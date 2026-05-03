using Application.DTOs.Shipments;
using Application.Models;
using Domain.Entities.Users;

namespace Application.Interfaces.Repositories.Shipments.User
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByApplicationUserIdAsync(string userId);
        Task<Customer?> GetDetailsByApplicationUserIdAsync(string userId);
        Task<IEnumerable<Customer>> GetAllAsync(CustomerParameters parameters);

        Task<bool> UserExistsAsync(string userId);
        Task<bool> ExistsByApplicationUserIdAsync(string userId);
        Task<bool> NationalIdExistsAsync(string nationalId, Guid? excludeCustomerId = null);
        Task<bool> TaxNumberExistsAsync(string taxNumber, string countryCode, Guid? excludeCustomerId = null);

        Task AddAsync(Customer customer);
    }
}