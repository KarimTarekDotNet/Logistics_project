using Application.Interfaces.Repositories.Payments;
using Application.Models;
using Domain.Entities.Payments;
using Infrastructure.Data.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Payment
{
    public class PaymentTransactionRepository : IPaymentTransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentTransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(PaymentTransaction transaction)
        {
            await _context.PaymentTransactions.AddAsync(transaction);
        }

        public async Task<List<PaymentTransaction>> GetAllAsync(QueryParameters query)
        {
            var transactions =
            _context.PaymentTransactions.Include(x => x.Invoice)
            .AsQueryable();

            if (!string.IsNullOrEmpty(query.Search))
            {
                var wordSearch = $"%{query.Search.Trim()}%";
                transactions = transactions.Where(x =>
                    EF.Functions.Like(x.FailureReason, wordSearch) ||
                    EF.Functions.Like(x.Currency, wordSearch) ||
                    EF.Functions.Like(x.Provider.ToString(), wordSearch) ||
                    EF.Functions.Like(x.Method.ToString(), wordSearch) ||
                    EF.Functions.Like(x.Status.ToString(), wordSearch));
            }

            if (!string.IsNullOrEmpty(query.SortBy))
            {
                transactions = query.SortBy switch
                {
                    "amount" => transactions.OrderBy(x => x.Amount),
                    "amount_desc" => transactions.OrderByDescending(x => x.Amount),
                    "createdAt" => transactions.OrderBy(x => x.CreatedAt),
                    "createdAt_desc" => transactions.OrderByDescending(x => x.CreatedAt),
                    _ => transactions
                };
            }

            return await transactions
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();
        }

        public void RemoveRange(IEnumerable<PaymentTransaction> transactions)
        {
            _context.PaymentTransactions.RemoveRange(transactions);
        }

        public async Task<PaymentTransaction?> GetByIdAsync(Guid id)
        {
            return await _context.PaymentTransactions
                .Include(x => x.Invoice)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<PaymentTransaction>?> GetByInvoiceIdAsync(Guid invoiceId)
        {
            return await _context.PaymentTransactions
                .Include(x => x.Invoice)
                .Where(x => x.InvoiceId == invoiceId)
                .ToListAsync();
        }

        public async Task<PaymentTransaction?> GetByProviderOrderIdAsync(string providerOrderId)
        {
            return await _context.PaymentTransactions
                .Include(x => x.Invoice)
                .FirstOrDefaultAsync(x => x.ProviderOrderId == providerOrderId);
        }

        public async Task<PaymentTransaction?> GetByIdToCurrentUserAsync(Guid id, string userId)
        {
            return await _context.PaymentTransactions
                .Include(x => x.Invoice)
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        }
    }
}