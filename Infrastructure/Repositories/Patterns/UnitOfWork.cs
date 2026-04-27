using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Repositories.Pricing.PricingEngine;
using Application.Interfaces.Repositories.Pricing.Quotation;
using Application.Interfaces.Repositories.ShippingCore;
using Domain.Entities.Pricing.PricingEngine;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Repositories.Patterns
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        public ICarrierRepository Carriers { get; }
        public IContainerTypeRepository ContainerTypes { get; }
        public IPortRepository Ports { get; }
        public IRouteRepository Routes { get; }
        public IRateRepository Rates { get; }
        public IQuoteRepository Quotes { get; }

        public UnitOfWork(
            ApplicationDbContext context,
            ICarrierRepository carrierRepository,
            IContainerTypeRepository containerTypeRepository,
            IPortRepository portRepository,
            IRouteRepository routeRepository,
            IRateRepository rateRepository,
            IQuoteRepository quoteRepository)
        {
            _context = context;
            Carriers = carrierRepository;
            ContainerTypes = containerTypeRepository;
            Ports = portRepository;
            Routes = routeRepository;
            Rates = rateRepository;
            Quotes = quoteRepository;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction == null) return;

            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null) return;

            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}