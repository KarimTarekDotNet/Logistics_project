using Application.Common;
using Application.DTOs.Shipments.User;
using Application.Models;

namespace Application.Interfaces.Services.Shipments.User
{
    public interface ICustomerService
    {
        Task<Result<CustomerResponse>> AddCustomerAsync(string userId, CreateCustomerRequest request);
        Task<Result<CustomerResponse>> UpdateCustomerAsync(string userId, UpdateCustomerRequest request);
        Task<Result<bool>> DeleteCustomerAsync(string userId);
        Task<Result<CustomerResponse>> GetByApplicationUserIdAsync(string userId);
        Task<Result<IEnumerable<CustomerResponse>>> GetAllAsync(CustomerParameters parameters);
    }
}
