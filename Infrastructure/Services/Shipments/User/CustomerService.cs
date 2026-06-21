using Application.Common;
using Application.DTOs.Shipments.User;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Shipments.ApisIntegrations;
using Application.Interfaces.Services.Shipments.User;
using Application.Models;
using AutoMapper;
using Domain.Entities.Users;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Shipments.User
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITaxVerificationService _taxVerificationService;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(ITaxVerificationService taxVerificationService, IMapper mapper, IUnitOfWork unitOfWork, ILogger<CustomerService> logger)
        {
            _taxVerificationService = taxVerificationService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<CustomerResponse>> AddCustomerAsync(string userId, CreateCustomerRequest request)
        {
            _logger.LogInformation("Creating customer profile for user {UserId}", userId);

            var userExists = await _unitOfWork.Customers.UserExistsAsync(userId);
            if (!userExists)
            {
                _logger.LogWarning("User {UserId} not found when creating customer profile", userId);
                return Result<CustomerResponse>.NotFound("User not found.");
            }

            var customerExists = await _unitOfWork.Customers.ExistsByApplicationUserIdAsync(userId);
            if (customerExists)
                return Result<CustomerResponse>.Failure("Customer profile already exists for this user.");

            if (!string.IsNullOrWhiteSpace(request.NationalId))
            {
                var nationalExists = await _unitOfWork.Customers.NationalIdExistsAsync(request.NationalId);
                if (nationalExists)
                    return Result<CustomerResponse>.Failure("Invalid national Id.");
            }

            if (!string.IsNullOrWhiteSpace(request.TaxNumber) && !string.IsNullOrWhiteSpace(request.CountryCode))
            {
                var countryCode = request.CountryCode.Trim().ToUpper();
                var taxNumber = request.TaxNumber.Trim().ToUpper();

                var taxValid = await _taxVerificationService.VerifyAsync(countryCode, taxNumber);
                if (!taxValid.IsValid)
                    return Result<CustomerResponse>.Failure("Invalid tax number.");

                var taxExists = await _unitOfWork.Customers.TaxNumberExistsAsync(taxNumber, countryCode);
                if (taxExists)
                    return Result<CustomerResponse>.Failure("Invalid tax number.");
            }

            var customer = _mapper.Map<Customer>(request);
            customer.CountryCode = !string.IsNullOrWhiteSpace(request.CountryCode) ? request.CountryCode.Trim().ToUpper() : null;
            customer.TaxNumber = !string.IsNullOrWhiteSpace(request.TaxNumber) ? request.TaxNumber.Trim().ToUpper() : null;
            customer.ApplicationUserId = userId;
            customer.CreatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Customer profile created for user {UserId}", userId);
            return Result<CustomerResponse>.Success(_mapper.Map<CustomerResponse>(customer), 201);
        }

        public async Task<Result<CustomerResponse>> UpdateCustomerAsync(string userId, UpdateCustomerRequest request)
        {
            _logger.LogInformation("Updating customer profile for user {UserId}", userId);

            var customer = await _unitOfWork.Customers.GetByApplicationUserIdAsync(userId);
            if (customer == null || customer.IsDeleted)
            {
                _logger.LogWarning("Customer not found for user {UserId}", userId);
                return Result<CustomerResponse>.NotFound("Customer not found.");
            }

            if (!request.DateOfBirth.HasValue) request.DateOfBirth = customer.DateOfBirth;
            if (string.IsNullOrWhiteSpace(request.CompanyName)) request.CompanyName = customer.CompanyName;
            if (string.IsNullOrWhiteSpace(request.NationalId)) request.NationalId = customer.NationalId;

            if (!string.IsNullOrWhiteSpace(request.NationalId))
            {
                var nationalExists = await _unitOfWork.Customers.NationalIdExistsAsync(request.NationalId, customer.Id);
                if (nationalExists)
                    return Result<CustomerResponse>.Failure("National Id already exists.");
            }

            var hasTax = !string.IsNullOrWhiteSpace(request.TaxNumber);
            var hasCountry = !string.IsNullOrWhiteSpace(request.CountryCode);

            if (hasTax != hasCountry)
                return Result<CustomerResponse>.Failure("Tax number and country code must be provided together.");

            if (!hasTax && !hasCountry)
            {
                request.TaxNumber = customer.TaxNumber;
                request.CountryCode = customer.CountryCode;
            }
            else
            {
                var countryCode = request.CountryCode!.Trim().ToUpper();
                var taxNumber = request.TaxNumber!.Trim().ToUpper();

                var taxValid = await _taxVerificationService.VerifyAsync(countryCode, taxNumber);
                if (!taxValid.IsValid)
                    return Result<CustomerResponse>.Failure("Invalid tax number.");

                var taxExists = await _unitOfWork.Customers.TaxNumberExistsAsync(taxNumber, countryCode, customer.Id);
                if (taxExists)
                    return Result<CustomerResponse>.Failure("Tax number already exists.");
            }

            _mapper.Map(request, customer);
            customer.CountryCode = !string.IsNullOrWhiteSpace(request.CountryCode) ? request.CountryCode.Trim().ToUpper() : null;
            customer.TaxNumber = !string.IsNullOrWhiteSpace(request.TaxNumber) ? request.TaxNumber.Trim().ToUpper() : null;
            customer.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Customer profile updated for user {UserId}", userId);
            return Result<CustomerResponse>.Success(_mapper.Map<CustomerResponse>(customer));
        }

        public async Task<Result<bool>> DeleteCustomerAsync(string userId)
        {
            _logger.LogInformation("Deleting customer profile for user {UserId}", userId);

            var customer = await _unitOfWork.Customers.GetDetailsByApplicationUserIdAsync(userId);
            if (customer == null || customer.IsDeleted)
            {
                _logger.LogWarning("Customer not found for user {UserId}", userId);
                return Result<bool>.NotFound("Customer not found.");
            }

            var hasActiveShipments = customer.Shipments.Any(s =>
                !s.IsDeleted &&
                s.Status != ShipmentStatus.Delivered &&
                s.Status != ShipmentStatus.Closed &&
                s.Status != ShipmentStatus.Cancelled);

            if (hasActiveShipments)
                return Result<bool>.Failure("Cannot delete customer with active shipments.");

            var hasActiveQuotes = customer.Quotes.Any(q => !q.IsDeleted);
            if (hasActiveQuotes)
                return Result<bool>.Failure("Cannot delete customer with active quotes.");

            customer.IsDeleted = true;
            customer.DeletedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Customer profile deleted for user {UserId}", userId);
            return Result<bool>.Success(true);
        }

        public async Task<Result<CustomerResponse>> GetByApplicationUserIdAsync(string userId)
        {
            var customer = await _unitOfWork.Customers.GetDetailsByApplicationUserIdAsync(userId);
            if (customer == null || customer.IsDeleted)
                return Result<CustomerResponse>.NotFound("Customer not found.");

            return Result<CustomerResponse>.Success(_mapper.Map<CustomerResponse>(customer));
        }

        public async Task<Result<IEnumerable<CustomerResponse>>> GetAllAsync(CustomerParameters parameters)
        {
            var customers = await _unitOfWork.Customers.GetAllAsync(parameters);
            return Result<IEnumerable<CustomerResponse>>.Success(_mapper.Map<IEnumerable<CustomerResponse>>(customers));
        }
    }
}
