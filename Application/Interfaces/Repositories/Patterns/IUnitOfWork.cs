using Application.Interfaces.Repositories.Pricing.PricingEngine;
using Application.Interfaces.Repositories.Pricing.Quotation;
using Application.Interfaces.Repositories.ShippingCore;
using Application.Interfaces.Services.Auth;

namespace Application.Interfaces.Repositories.Patterns
{
    public interface IUnitOfWork : IDisposable
    {
        ICarrierRepository Carriers { get; }
        IContainerTypeRepository ContainerTypes { get; }
        IPortRepository Ports { get; }
        IRouteRepository Routes { get; }
        IRateRepository Rates { get; }
        IQuoteRepository Quotes { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
