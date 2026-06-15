using Application.DTOs.User;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Repositories.Users;
using AutoMapper;
using Domain.Entities.Users.Subscriptions;
using Domain.Exceptions;

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

            newPlan.DurationInDays = request.DurationInDays;
            newPlan.CreatedAt = DateTimeOffset.UtcNow;
            newPlan.IsActive = true;
            newPlan.Currency = request.Currency.ToUpperInvariant();

            foreach (var createSubscriptionFeature in request.CreateSubscriptionFeatures)
            {
                var normalizedCode = createSubscriptionFeature.Code.Trim().ToUpperInvariant();
                var normalizedName = createSubscriptionFeature.Name.Trim();

                if (newPlan.PlanFeatures.Any(x =>
                    x.SubscriptionFeature.FeatureCode == normalizedCode))
                    throw new BusinessRuleException("Duplicate feature code in the same plan.");

                var feature = await _unitOfWork.SubscriptionPlans
                    .GetByCodeAsync(normalizedCode);

                if (feature == null)
                {
                    feature = new SubscriptionFeature
                    {
                        Id = Guid.NewGuid(),
                        FeatureCode = normalizedCode,
                        FeatureName = normalizedName
                    };
                }

                newPlan.PlanFeatures.Add(new SubscriptionPlanFeature
                {
                    Id = Guid.NewGuid(),
                    SubscriptionFeature = feature
                });
            }

            foreach (var createSubscriptionPlanLimit in request.CreateSubscriptionPlanLimits)
            {
                var code = createSubscriptionPlanLimit.Code.Trim().ToUpperInvariant();

                if (newPlan.PlanLimits.Any(x => x.LimitCodeSubscription.Equals(code, StringComparison.OrdinalIgnoreCase)))
                    throw new BusinessRuleException($"Duplicate limit code: {code}");

                newPlan.PlanLimits.Add(new SubscriptionPlanLimit
                {
                    Id = Guid.NewGuid(),
                    LimitCodeSubscription = code.ToUpperInvariant(),
                    LimitMaxValue = createSubscriptionPlanLimit.MaxValue
                });
            }


            await _unitOfWork.SubscriptionPlans.AddAsync(newPlan);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<SubscriptionPlanResponse>(newPlan);

            dto.SubscriptionFeatureResponses = _mapper.Map<ICollection<SubscriptionFeatureResponse>>(
                newPlan.PlanFeatures.Select(x => x.SubscriptionFeature));
            dto.SubscriptionPlanLimitResponses = _mapper.Map<ICollection<SubscriptionPlanLimitResponse>>(newPlan.PlanLimits);

            return dto;
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

        public async Task<SubscriptionPlanResponse> UpdateFromEmployeesAsync(Guid subscriptionPlanId, bool isEmployee,
        CreateSubscriptionPlanRequest? request = null)
        {
            if(!isEmployee)
                throw new UnauthorizedAccessException("Only employees can update subscription plans.");

            var plan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(subscriptionPlanId);
            if (plan == null || plan.IsDeleted || !plan.IsActive)
                throw new KeyNotFoundException("Subscription plan not found.");

            if(request == null)
                return _mapper.Map<SubscriptionPlanResponse>(plan);

            plan.UpdatedAt = DateTime.UtcNow;
            plan.IsActive = false;

            var newPlan = await AddFromEmployeesAsync(request, isEmployee);
            return newPlan;
        }
    }
}
