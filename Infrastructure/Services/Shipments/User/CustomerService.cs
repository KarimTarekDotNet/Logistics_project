using Application.DTOs.Shipments.User;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Shipments.ApisIntegrations;
using Application.Interfaces.Services.Shipments.User;
using Application.Models;
using AutoMapper;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Exceptions;

namespace Infrastructure.Services.Shipments.User
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITaxVerificationService _taxVerificationService;
        private readonly IMapper _mapper;

        public CustomerService(ITaxVerificationService taxVerificationService, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _taxVerificationService = taxVerificationService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<CustomerResponse> AddCustomerAsync(string userId, CreateCustomerRequest request)
        {
            var userExists = await _unitOfWork.Customers.UserExistsAsync(userId);
            if (!userExists)
                throw new KeyNotFoundException("User not found.");

            var customerExists = await _unitOfWork.Customers.ExistsByApplicationUserIdAsync(userId);
            if (customerExists)
                throw new BusinessRuleException("Customer profile already exists for this user.");

            if (!string.IsNullOrWhiteSpace(request.NationalId))
            {
                var nationalExists = await _unitOfWork.Customers.NationalIdExistsAsync(request.NationalId);
                if (nationalExists)
                    throw new BusinessRuleException("Invalid national Id.");
            }

            if (!string.IsNullOrWhiteSpace(request.TaxNumber) && !string.IsNullOrWhiteSpace(request.CountryCode))
            {
                var countryCode = request.CountryCode.Trim().ToUpper();
                var taxNumber = request.TaxNumber.Trim().ToUpper();

                var taxValid = await _taxVerificationService.VerifyAsync(countryCode, taxNumber);

                if (!taxValid.IsValid)
                    throw new BusinessRuleException("Invalid tax number.");

                var taxExists = await _unitOfWork.Customers.TaxNumberExistsAsync(taxNumber, countryCode);

                if (taxExists)
                    throw new BusinessRuleException("Invalid tax number.");
            }

            var customer = _mapper.Map<Customer>(request);

            customer.CountryCode = !string.IsNullOrWhiteSpace(request.CountryCode) ? request.CountryCode.Trim().ToUpper() : null;
            customer.TaxNumber = !string.IsNullOrWhiteSpace(request.TaxNumber) ? request.TaxNumber.Trim().ToUpper() : null;
            customer.ApplicationUserId = userId;
            customer.CreatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CustomerResponse>(customer);
        }

        public async Task<CustomerResponse> UpdateCustomerAsync(string userId, UpdateCustomerRequest request)
        {
            var customer = await _unitOfWork.Customers.GetByApplicationUserIdAsync(userId);

            if (customer == null || customer.IsDeleted)
                throw new KeyNotFoundException("Customer not found.");

            if (!request.DateOfBirth.HasValue)
                request.DateOfBirth = customer.DateOfBirth;

            if (string.IsNullOrWhiteSpace(request.CompanyName))
                request.CompanyName = customer.CompanyName;
            if (string.IsNullOrWhiteSpace(request.NationalId))
                request.NationalId = customer.NationalId;

            if (!string.IsNullOrWhiteSpace(request.NationalId))
            {
                var nationalExists = await _unitOfWork.Customers.NationalIdExistsAsync(request.NationalId, customer.Id);

                if (nationalExists)
                    throw new BusinessRuleException("National Id already exists.");
            }

            var hasTax = !string.IsNullOrWhiteSpace(request.TaxNumber);
            var hasCountry = !string.IsNullOrWhiteSpace(request.CountryCode);

            if (hasTax != hasCountry)
                throw new BusinessRuleException("Tax number and country code must be provided together.");

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
                    throw new BusinessRuleException("Invalid tax number.");

                var taxExists = await _unitOfWork.Customers.
                    TaxNumberExistsAsync(taxNumber, countryCode, customer.Id);

                if (taxExists)
                    throw new BusinessRuleException("Tax number already exists.");
            }

            _mapper.Map(request, customer);


            customer.CountryCode = !string.IsNullOrWhiteSpace(request.CountryCode) ? request.CountryCode.Trim().ToUpper() : null;
            customer.TaxNumber = !string.IsNullOrWhiteSpace(request.TaxNumber) ? request.TaxNumber.Trim().ToUpper() : null;
            customer.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CustomerResponse>(customer);
        }

        public async Task<bool> DeleteCustomerAsync(string userId)
        {
            var customer = await _unitOfWork.Customers.GetDetailsByApplicationUserIdAsync(userId);

            if (customer == null || customer.IsDeleted)
                throw new KeyNotFoundException("Customer not found.");

            var hasActiveShipments = customer.Shipments.Any(s =>
                !s.IsDeleted &&
                s.Status != ShipmentStatus.Delivered &&
                s.Status != ShipmentStatus.Closed &&
                s.Status != ShipmentStatus.Cancelled);

            if (hasActiveShipments)
                throw new BusinessRuleException("Cannot delete customer with active shipments.");

            var hasActiveQuotes = customer.Quotes.Any(q => !q.IsDeleted);

            if (hasActiveQuotes)
                throw new BusinessRuleException("Cannot delete customer with active quotes.");

            customer.IsDeleted = true;
            customer.DeletedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<CustomerResponse?> GetByApplicationUserIdAsync(string userId)
        {
            var customer = await _unitOfWork.Customers.GetDetailsByApplicationUserIdAsync(userId);

            return customer == null || customer.IsDeleted ? null : _mapper.Map<CustomerResponse>(customer);
        }

        public async Task<IEnumerable<CustomerResponse>> GetAllAsync(CustomerParameters parameters)
        {
            var customers = await _unitOfWork.Customers.GetAllAsync(parameters);

            return _mapper.Map<IEnumerable<CustomerResponse>>(customers);
        }
    }
}