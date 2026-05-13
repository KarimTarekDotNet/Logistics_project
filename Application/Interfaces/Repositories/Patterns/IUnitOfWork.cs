using Application.Interfaces.Repositories.Pricing.Imports;
using Application.Interfaces.Repositories.Pricing.PricingEngine;
using Application.Interfaces.Repositories.Pricing.Quotation;
using Application.Interfaces.Repositories.Shipments.Core;
using Application.Interfaces.Repositories.Shipments.User;
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
        ICustomerRepository Customers { get; }
        IShipmentRepository Shipments { get; }
        IShipmentItemRepository ShipmentItems { get; }
        IShipmentChargeRepository ShipmentCharges { get; }
        IShipmentStatusHistoryRepository StatusHistoryRepositories { get; }
        IIntegrationMessageRepository IntegrationMessage { get; }
        IInvoiceRepository Invoices { get; }
        IShipmentDocumentRepository ShipmentDocuments { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
