using Application.DTOs.User;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Repositories.Users;
using AutoMapper;
using Domain.Entities.Users;

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

        public async Task<SubscriptionPlanResponse> AddFromEmployeesAsync(CreateSubscriptionPlanRequest request, bool isEmployee)
        {
            if (!isEmployee)
                throw new UnauthorizedAccessException("Only employees can add subscription plans.");

            var newPlan = _mapper.Map<SubscriptionPlan>(request);

            newPlan.DurationInDays = 30;
            newPlan.CreatedAt = DateTimeOffset.UtcNow;
            newPlan.IsActive = true;

            await _unitOfWork.SubscriptionPlans.AddAsync(newPlan);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SubscriptionPlanResponse>(newPlan);
        }

        public async Task DeleteFromEmployeesAsync(Guid subscriptionPlanId, bool isEmployee)
        {
            if(!isEmployee)
                throw new UnauthorizedAccessException("Only employees can delete subscription plans.");

            var plan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(subscriptionPlanId);
            if (plan == null || plan.IsDeleted || !plan.IsActive)
                throw new KeyNotFoundException("Subscription plan not found.");

            plan.IsDeleted = true;
            plan.IsActive = false;
            plan.DeletedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IReadOnlyCollection<SubscriptionPlanResponse>> GetAllAsync()
        {
            var plans = await _unitOfWork.SubscriptionPlans.GetAllAsync();
            if(!plans.Any())
                return Array.Empty<SubscriptionPlanResponse>();

            return _mapper.Map<IReadOnlyCollection<SubscriptionPlanResponse>>(plans);
        }

        public async Task<SubscriptionPlanResponse> GetByIdAsync(Guid subscriptionPlanId)
        {
            var plan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(subscriptionPlanId);
            if (plan == null || plan.IsDeleted || !plan.IsActive)
                throw new KeyNotFoundException("Subscription plan not found.");

            return _mapper.Map<SubscriptionPlanResponse>(plan);
        }

        public async Task<SubscriptionPlanResponse> UpdateFromEmployeesAsync(Guid subscriptionPlanId,
        UpdateSubscriptionPlanRequest request, bool isEmployee)
        {
            if(!isEmployee)
                throw new UnauthorizedAccessException("Only employees can update subscription plans.");

            var plan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(subscriptionPlanId);
            if (plan == null || plan.IsDeleted || !plan.IsActive)
                throw new KeyNotFoundException("Subscription plan not found.");

            if (!string.IsNullOrWhiteSpace(request.Title))
                plan.Title = request.Title;

            if(!string.IsNullOrWhiteSpace(request.Description))
                plan.Description = request.Description;

            if(request.DurationInDays.HasValue)
                plan.DurationInDays = request.DurationInDays.Value;

            if(request.Price.HasValue)
                plan.Price = request.Price.Value;

            plan.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<SubscriptionPlanResponse>(plan);
        }
    }
}
