using Application.Interfaces.Repositories.Aliases;
using Domain.Entities.Aliases;
using Domain.Enums;
using Infrastructure.Data.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Aliases
{
    public class AliasRepository : IAliasRepository
    {
        private readonly ApplicationDbContext _context;

        public AliasRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Add(Alias alias)
        {
            await _context.Aliases.AddAsync(alias);
        }

        public async Task<bool> EntityExistsAsync(Guid id, AliasType type)
        {
            return type switch
            {
                AliasType.Carrier => await _context.Carriers.AnyAsync(x => x.Id == id),
                AliasType.Port => await _context.Ports.AnyAsync(x => x.Id == id),
                AliasType.ContainerType => await _context.ContainerTypes.AnyAsync(x => x.Id == id),
                _ => false
            };
        }

        public async Task<Alias?> GetById(Guid id)
        {
            return await _context.Aliases.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Alias?> GetByName(string name)
        {
            var convertNameToEnableSearchInDb = $"%{name.Trim()}%";
            return await _context.Aliases.FirstOrDefaultAsync(x =>
            EF.Functions.Like(x.AliasName, convertNameToEnableSearchInDb));
        }

        public async Task<Alias?> GetByNormalizedNameAsync(string normalizedName, AliasType type)
        {
            return await _context.Aliases.FirstOrDefaultAsync(x => x.Type == type && x.NormalizedAlias == normalizedName && !x.IsDeleted);
        }

        public void Update(Alias alias)
        {
            _context.Aliases.Update(alias);
        }

        public void Remove(Alias alias)
        {
            alias.IsDeleted = true;
            alias.DeletedAt = DateTimeOffset.UtcNow;
        }
    }
}
