using Application.ApplicationRules.Shipments;
using Application.DTOs.Payment;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Payment;
using Application.Interfaces.Services.System;
using AutoMapper;
using Domain.Entities.Payments;
using Domain.Entities.Shipments;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Exceptions;
using Infrastructure.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services.Payment
{
    public class PaymentTransactionService : IPaymentTransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPaymobPaymentService _paymobPaymentService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IRedisService _redisService;

        public PaymentTransactionService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager,
        IPaymobPaymentService paymobPaymentService, IMapper mapper, IConfiguration configuration, IRedisService redisService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _paymobPaymentService = paymobPaymentService;
            _mapper = mapper;
            _configuration = configuration;
            _redisService = redisService;
        }

        public async Task<StartPaymentResponse> StartPaymentAsync(StartPaymentRequest request, string userId)
        {
            try
            {
                var user = await _userManager.Users.Include(x => x.CustomerProfile)
                .FirstOrDefaultAsync(x => x.Id == userId);

                if (user == null || user.CustomerProfile == null)
                    throw new InvalidOperationException("User not found.");

                var (invoice, shipment) = await InvoiceHelper
                .GetInvoiceContextAsync(request.InvoiceId, _unitOfWork);

                if (invoice == null)
                    throw new BusinessRuleException("Associated invoice not found.");

                if (!ShipmentStatusRules.CanPayInvoice(shipment.Status))
                    throw new BusinessRuleException("Cannot pay invoice for the current shipment status.");

                InvoiceHelper.EnsureInvoiceCanBePaid(invoice);

                if (invoice.PaymentStatus == PaymentStatus.Paid)
                    throw new BusinessRuleException("Invoice is already paid.");

                if (invoice.PaymentStatus == PaymentStatus.Cancelled)
                    throw new BusinessRuleException("Invoice is cancelled.");

                if (invoice.PaymentStatus == PaymentStatus.Draft)
                    throw new BusinessRuleException("Invoice is in draft status.");


                var redisKey = $"idempotency:payment:invoice:{invoice.Id}:user:{user.Id}";
                var paymentTransactionId = Guid.NewGuid();

                var acquired = await _redisService.TryAcquireIdempotencyKeyAsync(redisKey, paymentTransactionId.ToString(),
                TimeSpan.FromMinutes(15));

                if (!acquired)
                {
                    var existingTransactionId = await _redisService.GetAsync<string>(redisKey);
                    var existingTransaction = 
                    await _unitOfWork.PaymentTransactions.GetByIdToCurrentUserAsync(Guid.Parse(existingTransactionId!), userId);

                    if (existingTransaction == null)
                        throw new BusinessRuleException("Payment transaction not found.");

                    if (string.IsNullOrWhiteSpace(existingTransaction.ClientSecret))
                        throw new BusinessRuleException("Payment is still being initialized. Please retry shortly.");

                    return new StartPaymentResponse
                    {
                        PaymentTransactionId = Guid.Parse(existingTransactionId!),
                        ClientSecret = existingTransaction.ClientSecret,
                        Status = existingTransaction.Status
                    };
                }

                var paymentTransaction = new PaymentTransaction
                {
                    Id = paymentTransactionId,
                    UserId = user.Id,
                    InvoiceId = invoice.Id,
                    FailureReason = null,
                    GatewayResponse = null,
                    Amount = invoice.TotalAmount,
                    Currency = invoice.Currency,
                    Method = PaymentMethod.CreditCard,
                    Provider = PaymentProvider.Paymob,
                    Status = PaymentTransactionStatus.Pending,
                    CreatedAt = DateTimeOffset.UtcNow
                };
                await _unitOfWork.PaymentTransactions.AddAsync(paymentTransaction);
                await _unitOfWork.SaveChangesAsync();

                var amountCents = PaymentHelper.ConvertToCentsFromUSDToEGY(invoice.TotalAmount);
                var intentionRequest = new CreatePaymobIntentionRequest
                {
                    Amount = amountCents, // Convert to the smallest currency unit
                    Currency = "EGP",
                    BillingData = new PaymobBillingDataRequest
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email!,
                        PhoneNumber = user.PhoneNumber ?? "01000000000",
                        Apartment = "NA",
                        Floor = "NA",
                        Street = "NA",
                        Building = "NA",
                        City = "NA",
                        State = "NA",
                        Country = "EG"
                    },
                    Items = new List<PaymobItemRequest>
                {
                    new PaymobItemRequest
                    {
                        Name = $"Invoice #{invoice.Id}",
                        Quantity = 1,
                        Amount = amountCents,
                        Description = $"Payment for Invoice #{invoice.Id}"
                    }
                },
                    PaymentMethods = new List<int> { int.Parse(_configuration.GetValue<string>("Paymob:PaymentMethod")!) },
                    SpecialReference = $"payment_{paymentTransaction.Id}",
                    NotificationUrl = _configuration.GetValue<string>("Paymob:CallbackUrl")!,
                    RedirectionUrl = _configuration.GetValue<string>("Paymob:RedirectUrl")!
                };

                var paymobResult = await _paymobPaymentService.CreateIntentionAsync(intentionRequest);

                paymentTransaction.ProviderIntentionId = paymobResult.IntentionId;
                paymentTransaction.ProviderOrderId = paymobResult.OrderId.ToString();
                paymentTransaction.ClientSecret = paymobResult.ClientSecret;

                await _unitOfWork.SaveChangesAsync();

                return new StartPaymentResponse
                {
                    PaymentTransactionId = paymentTransaction.Id,
                    ClientSecret = paymobResult.ClientSecret,
                    Status = paymentTransaction.Status
                };
            }
            catch (Exception)
            {
                // Log the exception (not implemented here)
                var redisKey = $"idempotency:payment:invoice:{request.InvoiceId}:user:{userId}";
                var existingTransactionId = await _redisService.GetAsync<string>(redisKey);
                if (existingTransactionId != null)
                    await _redisService.RemoveAsync(redisKey);

                var transactions = await _unitOfWork.PaymentTransactions.GetByInvoiceIdAsync(request.InvoiceId);
                if (transactions != null)
                {
                    _unitOfWork.PaymentTransactions.RemoveRange(transactions);
                }
                throw;
            }
        }

        public async Task<PaymentTransactionResponse?> GetByIdAsync(Guid id, string userId, bool isPrivileged)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found.");

            if(isPrivileged)
                return _mapper.Map<PaymentTransactionResponse>(await _unitOfWork.PaymentTransactions.GetByIdAsync(id));

            else
            {
                var transaction = await _unitOfWork.PaymentTransactions.GetByIdToCurrentUserAsync(id, userId);
                return _mapper.Map<PaymentTransactionResponse>(transaction);
            }
        }

        public async Task HandlePaymobWebhookAsync(PaymobTransactionWebhookRequest request, string receivedHmac)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(receivedHmac))
                    throw new BusinessRuleException("Invalid webhook data.");

                if (request.Obj == null)
                    throw new BusinessRuleException("Invalid webhook object.");

                if (request.Obj.TransactionId <= 0)
                    throw new BusinessRuleException("Missing transaction ID in webhook.");

                if (request.Obj.Order == null || request.Obj.Order.OrderId <= 0)
                    throw new BusinessRuleException("Missing order ID in webhook.");

                var paymentTransaction = await _unitOfWork.PaymentTransactions
                .GetByProviderOrderIdAsync(request.Obj.Order.OrderId.ToString());

                if (paymentTransaction == null)
                    throw new BusinessRuleException("Payment transaction not found for the given order ID.");

                if (!IsValidHmac(request, receivedHmac))
                    throw new BusinessRuleException("Invalid HMAC signature in webhook.");

                if (paymentTransaction.Status != PaymentTransactionStatus.Pending)
                    return;

                if (request.Obj.Currency != "EGP")
                    throw new BusinessRuleException("Currency mismatch between webhook and payment transaction.");

                if (request.Obj.AmountCents != PaymentHelper.ConvertToCentsFromUSDToEGY(paymentTransaction.Amount))
                    throw new BusinessRuleException("Amount mismatch between webhook and payment transaction.");

                if (request.Obj.Success)
                {
                    var paidAt = DateTimeOffset.UtcNow;

                    paymentTransaction.Status = PaymentTransactionStatus.Succeeded;
                    paymentTransaction.ProviderTransactionId = request.Obj.TransactionId.ToString();
                    paymentTransaction.GatewayResponse = $"Payment succeeded with Paymob. Transaction ID: {request.Obj.TransactionId}";
                    paymentTransaction.PaidAt = paidAt;

                    var invoicePayment = await _unitOfWork.InvoicePayments
                    .GetByTransactionIdAsync(paymentTransaction.ProviderTransactionId);
                    if (invoicePayment != null)
                    {
                        invoicePayment.Status = PaymentTransactionStatus.Succeeded;
                        invoicePayment.TransactionId = paymentTransaction.ProviderTransactionId;
                        invoicePayment.PaidAt = paidAt;
                        invoicePayment.ReferenceNumber = paymentTransaction.Id.ToString();
                    }
                    else
                    {
                        var newInvoicePayment = new InvoicePayment
                        {
                            InvoiceId = paymentTransaction.InvoiceId!.Value,
                            Amount = paymentTransaction.Amount,
                            Currency = paymentTransaction.Currency,
                            CreatedAt = paymentTransaction.CreatedAt,
                            Status = PaymentTransactionStatus.Succeeded,
                            PaymentMethod = paymentTransaction.Method,
                            PaymentProvider = paymentTransaction.Provider,
                            ReferenceNumber = paymentTransaction.Id.ToString(),
                            TransactionId = paymentTransaction.ProviderTransactionId,
                            PaidAt = DateTimeOffset.UtcNow
                        };
                        await _unitOfWork.InvoicePayments.AddAsync(newInvoicePayment);
                    }

                    var invoice = await _unitOfWork.Invoices.GetByIdAsync(paymentTransaction.InvoiceId!.Value);
                    if (invoice != null)
                    {
                        invoice.PaymentStatus = PaymentStatus.Paid;
                        invoice.PaidAt = paidAt;
                        invoice.UpdatedAt = paidAt;
                    }
                }
                else
                {
                    paymentTransaction.Status = PaymentTransactionStatus.Failed;
                    paymentTransaction.GatewayResponse = $"Payment failed with Paymob. Transaction ID: {request.Obj.TransactionId}";
                    paymentTransaction.FailureReason = "Payment failed according to Paymob webhook.";
                }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                throw new Exception(ex.Message);
            }
        }

        public async Task CancelPendingPaymentAsync(Guid paymentTransactionId, string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    throw new InvalidOperationException("User not found.");

                var transaction = await _unitOfWork.PaymentTransactions.GetByIdToCurrentUserAsync(paymentTransactionId, userId);
                if(transaction == null)
                    throw new BusinessRuleException("Payment transaction not found.");

                if(transaction.Status != PaymentTransactionStatus.Pending)
                    throw new BusinessRuleException("Only pending transactions can be cancelled.");

                transaction.Status = PaymentTransactionStatus.Cancelled;
                transaction.GatewayResponse = "Payment cancelled by user.";

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Log the exception or handle it as needed
                throw;
            }
        }

        public async Task<CheckoutPaymentResponse> CheckoutAsync(Guid paymentTransactionId, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found.");

            var transaction = await _unitOfWork.PaymentTransactions.GetByIdToCurrentUserAsync(paymentTransactionId, userId);
            if (transaction == null)
                throw new BusinessRuleException("Payment transaction not found.");

            if(transaction.Status != PaymentTransactionStatus.Pending)
                throw new BusinessRuleException("Only pending transactions can be checked out.");

            if(transaction.ClientSecret == null)
                throw new BusinessRuleException("Payment transaction is not ready for checkout.");

            var checkoutBaseUrl = _configuration.GetValue<string>("Paymob:UnifiedCheckoutUrl");
            var publicKey = _configuration.GetValue<string>("Paymob:PublicKey");

            if (string.IsNullOrWhiteSpace(checkoutBaseUrl))
                throw new InvalidOperationException("Paymob UnifiedCheckoutUrl is not configured.");

            if (string.IsNullOrWhiteSpace(publicKey))
                throw new InvalidOperationException("Paymob PublicKey is not configured.");

            var checkoutUrl =
                $"{checkoutBaseUrl}?publicKey={Uri.EscapeDataString(publicKey)}" +
                $"&clientSecret={Uri.EscapeDataString(transaction.ClientSecret)}";

            return new CheckoutPaymentResponse
            {
                CheckoutUrl = checkoutUrl
            };
        }
        private static string B(bool value)
            => value ? "true" : "false";

        private static string S(object? value)
            => value?.ToString() ?? "";
        private string BuildHmacPayload(PaymobTransactionWebhookRequest request)
        {
            var obj = request.Obj;

            return string.Concat(
                S(obj.AmountCents),
                S(obj.CreatedAt),
                S(obj.Currency),
                B(obj.ErrorOccured),
                B(obj.HasParentTransaction),
                S(obj.TransactionId),
                S(obj.IntegrationId),
                B(obj.Is3DSecure),
                B(obj.IsAuth),
                B(obj.IsCapture),
                B(obj.IsRefunded),
                B(obj.IsStandalonePayment),
                B(obj.IsVoided),
                S(obj.Order.OrderId),
                S(obj.Owner),
                B(obj.Pending),
                S(obj.SourceData.Pan),
                S(obj.SourceData.SubType),
                S(obj.SourceData.Type),
                B(obj.Success)
            );
        }

        private bool IsValidHmac(PaymobTransactionWebhookRequest request, string receivedHmac)
        {
            var payload = BuildHmacPayload(request);

            var secretKey = _configuration.GetValue<string>("Paymob:HMAC")!;
            if (string.IsNullOrWhiteSpace(secretKey))
                throw new InvalidOperationException("HMAC secret key is not configured.");

            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey));

            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var calculatedHmac = BitConverter.ToString(hash).Replace("-", "").ToLower();

            return string.Equals(calculatedHmac, receivedHmac, StringComparison.OrdinalIgnoreCase);
        }
    }
}