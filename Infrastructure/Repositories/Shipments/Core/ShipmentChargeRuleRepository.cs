using Application.Interfaces.Repositories.Shipments.Core;
using Domain.Entities.Shipments;
using Infrastructure.Data.Database;

namespace Infrastructure.Repositories.Shipments.Core
{
    public class ShipmentChargeRuleRepository : IShipmentChargeRuleRepository
    {
        private readonly ApplicationDbContext _context;

        public ShipmentChargeRuleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ShipmentChargeRule>> GetActiveRulesAsync(string currency)
        {
            return _context.ShipmentChargeRules.Where(x => x.IsActive && x.Currency == currency);
        }
    }
}
