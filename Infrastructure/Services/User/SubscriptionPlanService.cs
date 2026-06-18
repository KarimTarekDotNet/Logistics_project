using Application.DTOs.User;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Repositories.Users;
using AutoMapper;

namespace Infrastructure.Services.User
{
    public class SubscriptionPlanService : ISubscriptionPlanService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SubscriptionPlanService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public Task<SubscriptionPlanResponse> AddFromEmployeesAsync(CreateSubscriptionPlanRequest request, bool isEmployee)
        {
            throw new NotImplementedException();
        }

        public Task DeleteFromEmployeesAsync(Guid subscriptionPlanId, bool isEmployee)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<SubscriptionPlanResponse>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SubscriptionPlanResponse> GetByIdAsync(Guid subscriptionPlanId)
        {
            throw new NotImplementedException();
        }

        public Task<SubscriptionPlanResponse> UpdateFromEmployeesAsync(Guid subscriptionPlanId, bool isEmployee, CreateSubscriptionPlanRequest? request = null)
        {
            throw new NotImplementedException();
        }
    }
}
