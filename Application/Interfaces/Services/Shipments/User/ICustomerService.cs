using Application.DTOs.Shipments.User;
using Application.Models;

namespace Application.Interfaces.Services.Shipments.User
{
    public interface ICustomerService
    {
        Task<CustomerResponse> AddCustomerAsync(string userId, CreateCustomerRequest request);
        Task<CustomerResponse> UpdateCustomerAsync(string userId, UpdateCustomerRequest request);
        Task<bool> DeleteCustomerAsync(string userId);
        Task<CustomerResponse?> GetByApplicationUserIdAsync(string userId);
        Task<IEnumerable<CustomerResponse>> GetAllAsync(CustomerParameters parameters);
    }
}