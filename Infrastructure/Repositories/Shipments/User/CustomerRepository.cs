using Application.Interfaces.Repositories.Shipments.User;
using Application.Models;
using Domain.Entities.Users;
using Infrastructure.Data.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Shipments
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UserExistsAsync(string userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId);
        }

        public async Task<bool> ExistsByApplicationUserIdAsync(string userId)
        {
            return await _context.Customers
                .AnyAsync(c => c.ApplicationUserId == userId && !c.IsDeleted);
        }

        public async Task<Customer?> GetByApplicationUserIdAsync(string userId)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId && !c.IsDeleted);
        }

        public async Task<Customer?> GetDetailsByIdAsync(Guid customerId)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == customerId && !c.IsDeleted);
        }

        public async Task<Customer?> GetDetailsByApplicationUserIdAsync(string userId)
        {
            return await _context.Customers
                .AsNoTracking()
                .AsSplitQuery()
                .Include(c => c.Quotes).ThenInclude(q => q.ContainerType)
                .Include(c => c.Quotes).ThenInclude(q => q.Route).ThenInclude(r => r.FromPort)
                .Include(c => c.Quotes).ThenInclude(q => q.Route).ThenInclude(r => r.ToPort)
                .Include(c => c.Quotes).ThenInclude(q => q.Items)
                .Include(c => c.ApplicationUser)
                .Include(c => c.Shipments).ThenInclude(s => s.ContainerType)
                .Include(c => c.Shipments).ThenInclude(s => s.Carrier)
                .Include(c => c.Shipments).ThenInclude(s => s.Charges)
                .Include(c => c.Shipments).ThenInclude(s => s.Route)
                .Include(c => c.Shipments).ThenInclude(s => s.Quote)
                .Include(c => c.Shipments).ThenInclude(s => s.Items)
                .Include(c => c.Shipments).ThenInclude(s => s.StatusHistory)
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId && !c.IsDeleted);
        }

        public async Task<IEnumerable<Customer>> GetAllAsync(CustomerParameters parameters)
        {
            var query = _context.Customers
                .AsNoTracking()
                .AsSplitQuery()
                .Include(c => c.Quotes).ThenInclude(q => q.ContainerType)
                .Include(c => c.Quotes).ThenInclude(q => q.Route).ThenInclude(r => r.FromPort)
                .Include(c => c.Quotes).ThenInclude(q => q.Route).ThenInclude(r => r.ToPort)
                .Include(c => c.Quotes).ThenInclude(q => q.Items)
                .Include(c => c.ApplicationUser)
                .Include(c => c.Shipments).ThenInclude(s => s.ContainerType)
                .Include(c => c.Shipments).ThenInclude(s => s.Carrier)
                .Include(c => c.Shipments).ThenInclude(s => s.Charges)
                .Include(c => c.Shipments).ThenInclude(s => s.Route)
                .Include(c => c.Shipments).ThenInclude(s => s.Quote)
                .Include(c => c.Shipments).ThenInclude(s => s.Items)
                .Include(c => c.Shipments).ThenInclude(s => s.StatusHistory)
                .Where(c => !c.IsDeleted);

            if (parameters.DateOfBirth.HasValue)
                query = query.Where(c => c.DateOfBirth == parameters.DateOfBirth.Value);

            if (parameters.CreatedFrom.HasValue)
                query = query.Where(c => c.CreatedAt >= parameters.CreatedFrom.Value);

            if (parameters.CreatedTo.HasValue)
                query = query.Where(c => c.CreatedAt <= parameters.CreatedTo.Value);


            if (parameters.DeletedFrom.HasValue)
                query = query.Where(c => c.DeletedAt >= parameters.DeletedFrom.Value);

            if (parameters.DeletedTo.HasValue)
                query = query.Where(c => c.DeletedAt <= parameters.DeletedTo.Value);

            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                var term = $"%{parameters.Search.Trim()}%";

                query = query.Where(c =>
                    EF.Functions.Like(c.NationalId!, term) ||
                    EF.Functions.Like(c.CompanyName!, term) ||
                    EF.Functions.Like(c.TaxNumber!, term) ||
                    EF.Functions.Like(c.CountryCode!, term) ||
                    EF.Functions.Like(c.ApplicationUser.UserName!, term) ||
                    EF.Functions.Like(c.ApplicationUser.Email!, term) ||
                    EF.Functions.Like(c.ApplicationUser.PhoneNumber!, term)
                );
            }

            query = parameters.SortBy?.ToLower() switch
            {
                "createdat_asc" => query.OrderBy(c => c.CreatedAt),
                "createdat_desc" => query.OrderByDescending(c => c.CreatedAt),

                "dateofbirth_asc" => query.OrderBy(c => c.DateOfBirth),
                "dateofbirth_desc" => query.OrderByDescending(c => c.DateOfBirth),

                "companyname_asc" => query.OrderBy(c => c.CompanyName),
                "companyname_desc" => query.OrderByDescending(c => c.CompanyName),

                "countrycode_asc" => query.OrderBy(c => c.CountryCode),
                "countrycode_desc" => query.OrderByDescending(c => c.CountryCode),

                _ => query.OrderByDescending(c => c.CreatedAt)
            };

            return await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();
        }

        public async Task<bool> NationalIdExistsAsync(string nationalId, Guid? excludeCustomerId = null)
        {
            return await _context.Customers.AnyAsync(c =>
                c.NationalId == nationalId &&
                !c.IsDeleted &&
                (!excludeCustomerId.HasValue || c.Id != excludeCustomerId.Value));
        }

        public async Task<bool> TaxNumberExistsAsync(string taxNumber, string countryCode, Guid? excludeCustomerId = null)
        {
            return await _context.Customers.AnyAsync(c =>
                c.TaxNumber == taxNumber &&
                c.CountryCode == countryCode &&
                !c.IsDeleted &&
                (!excludeCustomerId.HasValue || c.Id != excludeCustomerId.Value));
        }

        public async Task AddAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
        }
    }
}