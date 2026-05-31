using Application.Interfaces.Repositories.Aliases;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Repositories.Pricing.Imports;
using Application.Interfaces.Repositories.Pricing.PricingEngine;
using Application.Interfaces.Repositories.Pricing.Quotation;
using Application.Interfaces.Repositories.Shipments.Core;
using Application.Interfaces.Repositories.Shipments.User;
using Application.Interfaces.Repositories.ShippingCore;
using Infrastructure.Data.Database;
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
        public ICustomerRepository Customers { get; }
        public IShipmentRepository Shipments { get; }
        public IShipmentItemRepository ShipmentItems { get; }
        public IShipmentChargeRepository ShipmentCharges { get; }
        public IShipmentStatusHistoryRepository StatusHistoryRepositories { get; }
        public IIntegrationMessageRepository IntegrationMessage { get; }
        public IInvoiceRepository Invoices { get; }
        public IShipmentDocumentRepository ShipmentDocuments { get; }
        public IAliasRepository Alias { get; }
        public IQuoteRequestRepository QuoteRequest { get; }
        public IShipmentChargeRuleRepository ShipmentChargeRule { get; }
        public IInvoicePaymentRepository InvoicePayments { get; }

        public UnitOfWork(
            ApplicationDbContext context,
            ICarrierRepository carrierRepository,
            IContainerTypeRepository containerTypeRepository,
            IPortRepository portRepository,
            IRouteRepository routeRepository,
            IRateRepository rateRepository,
            IQuoteRepository quoteRepository,
            ICustomerRepository customerRepository,
            IShipmentRepository shipmentRepository,
            IShipmentItemRepository shipmentItemRepository,
            IShipmentChargeRepository shipmentChargeRepository,
            IShipmentStatusHistoryRepository shipmentStatusHistoryRepository,
            IIntegrationMessageRepository integrationMessage,
            IInvoiceRepository invoiceRepository,
            IShipmentDocumentRepository shipmentDocuments,
            IAliasRepository alias,
            IQuoteRequestRepository quoteRequest,
            IShipmentChargeRuleRepository shipmentChargeRule,
            IInvoicePaymentRepository invoicePayments)
        {
            _context = context;
            Carriers = carrierRepository;
            ContainerTypes = containerTypeRepository;
            Ports = portRepository;
            Routes = routeRepository;
            Rates = rateRepository;
            Quotes = quoteRepository;
            Customers = customerRepository;
            Shipments = shipmentRepository;
            ShipmentItems = shipmentItemRepository;
            ShipmentCharges = shipmentChargeRepository;
            StatusHistoryRepositories = shipmentStatusHistoryRepository;
            IntegrationMessage = integrationMessage;
            Invoices = invoiceRepository;
            ShipmentDocuments = shipmentDocuments;
            Alias = alias;
            QuoteRequest = quoteRequest;
            ShipmentChargeRule = shipmentChargeRule;
            InvoicePayments = invoicePayments;
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