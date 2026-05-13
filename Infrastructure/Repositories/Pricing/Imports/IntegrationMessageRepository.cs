using Application.Interfaces.Repositories.Pricing.Imports;
using Domain.Entities.Pricing.Imports;
using Domain.Enums;
using Infrastructure.Data.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Pricing.Imports
{
    public class IntegrationMessageRepository : IIntegrationMessageRepository
    {
        private readonly ApplicationDbContext _context;

        public IntegrationMessageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(IntegrationMessage entity)
        {
            await _context.IntegrationMessages.AddAsync(entity);
        }

        public async Task<bool> ExistsAsync(string externalMessageId, ExternalSource source)
        {
            return await _context.IntegrationMessages.AnyAsync(im => im.ExternalMessageId == externalMessageId && im.Source == source);
        }

        public async Task<IntegrationMessage?> GetByExternalMessageIdAndSourceAsync(string externalMessageId, ExternalSource source)
        {
            return await _context.IntegrationMessages.FirstOrDefaultAsync(x => x.ExternalMessageId == externalMessageId && x.Source == source);
        }
    }
}
