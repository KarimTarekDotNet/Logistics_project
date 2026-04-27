using Application.Interfaces.Repositories.ShippingCore;
using Application.Models;
using Domain.Entities.ShippingCore;
using Infrastructure.Data;
using Infrastructure.Repositories.Patterns;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Pricing.ShippingCore
{
    public class ContainerTypeRepository : GenericRepository<ContainerType>, IContainerTypeRepository
    {
        private readonly ApplicationDbContext _context;
        public ContainerTypeRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ContainerType?> GetByNameAsync(string input)
        {
            var normalizedInput = input.Trim().ToLower();

            return await _context.ContainerTypes
                .FirstOrDefaultAsync(c => EF.Functions.Like(c.Name, normalizedInput));
        }

        public async Task<IEnumerable<ContainerType>> GetAllAsync(QueryParameters query)
        {
            var containerTypesQuery = _context.ContainerTypes
                .Where(ct => !ct.IsDeleted)
                .AsQueryable();
            if (!string.IsNullOrEmpty(query.Search))
            {
                var searchTerm = $"%{query.Search}%";
                containerTypesQuery = containerTypesQuery
                    .Where(ct => EF.Functions.Like(ct.Name, searchTerm));
            }

            containerTypesQuery = query.SortBy?.ToLower() switch
            {
                "name" => containerTypesQuery.OrderBy(ct => ct.Name),
                "name_desc" => containerTypesQuery.OrderByDescending(ct => ct.Name),
                "createdat" => containerTypesQuery.OrderByDescending(ct => ct.CreatedAt),
                "createdat_asc" => containerTypesQuery.OrderBy(ct => ct.CreatedAt),
                _ => containerTypesQuery.OrderByDescending(ct => ct.CreatedAt)
            };

            return await containerTypesQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();
        }
    }
}