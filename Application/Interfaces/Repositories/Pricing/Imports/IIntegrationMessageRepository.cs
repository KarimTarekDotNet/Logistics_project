using Application.Interfaces.Repositories.Patterns;
using Domain.Entities.Pricing.Imports;
using Domain.Enums;

namespace Application.Interfaces.Repositories.Pricing.Imports
{
    public interface IIntegrationMessageRepository
    {
        Task<bool> ExistsAsync(string externalMessageId, ExternalSource source);
        Task AddAsync(IntegrationMessage entity);
    }
}