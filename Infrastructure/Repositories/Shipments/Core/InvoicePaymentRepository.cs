using Application.Interfaces.Repositories.Shipments.Core;
using Domain.Entities.Shipments;
using Domain.Enums;
using Infrastructure.Data.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Shipments.Core
{
    public class InvoicePaymentRepository : IInvoicePaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public InvoicePaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(InvoicePayment payment)
        {
            await _context.InvoicePayments.AddAsync(payment);
        }

        public async Task<InvoicePayment?> GetByIdAsync(Guid id)
        {
            return await _context.InvoicePayments
                .Include(x => x.Invoice)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<InvoicePayment?> GetByTransactionIdAsync(string transactionId)
        {
            return await _context.InvoicePayments
                .FirstOrDefaultAsync(x => x.TransactionId == transactionId);
        }

        public async Task<InvoicePayment?> GetByReferenceNumberAsync(string referenceNumber)
        {
            return await _context.InvoicePayments
                .FirstOrDefaultAsync(x => x.ReferenceNumber == referenceNumber);
        }

        public async Task<IReadOnlyList<InvoicePayment>> GetByInvoiceIdAsync(Guid invoiceId, Guid customerId)
        {
            return await _context.InvoicePayments
                .AsNoTracking()
                .Where(x => x.InvoiceId == invoiceId && x.Invoice.Shipment.CustomerId == customerId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<InvoicePayment>> GetByInvoiceIdAsync(Guid invoiceId)
        {
            return await _context.InvoicePayments
                .Where(x => x.InvoiceId == invoiceId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
    }
}