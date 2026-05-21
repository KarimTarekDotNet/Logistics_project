using Domain.Entities.Shipments;

namespace Application.Interfaces.Repositories.Shipments.Core
{
    public interface IShipmentChargeRuleRepository
    {
        Task<IEnumerable<ShipmentChargeRule>> GetActiveRulesAsync(string currency);
    }
}