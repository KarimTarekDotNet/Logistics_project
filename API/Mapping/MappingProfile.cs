using Application.DTOs.Auth;
using Application.DTOs.Pricing.PricingEngine;
using Application.DTOs.Pricing.Quotation;
using Application.DTOs.Shipments.Core;
using Application.DTOs.Shipments.User;
using Application.DTOs.ShippingCore;
using Application.DTOs.User;
using AutoMapper;
using Domain.Entities.Pricing.Imports;
using Domain.Entities.Pricing.PricingEngine;
using Domain.Entities.Pricing.Quotation;
using Domain.Entities.Shipments;
using Domain.Entities.ShippingCore;
using Domain.Entities.Users;
using DomainRoute = Domain.Entities.ShippingCore.Route;

namespace API.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ── ShippingCore ─────────────────────────────────────────────────
            CreateMap<Port, PortResponse>();
            CreateMap<CreatePortRequest, Port>();
            CreateMap<UpdatePortRequest, Port>();

            CreateMap<Carrier, CarrierResponse>();
            CreateMap<CreateCarrierRequest, Carrier>();
            CreateMap<UpdateCarrierRequest, Carrier>();

            CreateMap<ContainerType, ContainerTypeResponse>();
            CreateMap<CreateContainerTypeRequest, ContainerType>();
            CreateMap<UpdateContainerTypeRequest, ContainerType>();

            CreateMap<DomainRoute, RouteResponse>()
                .ForMember(d => d.FromPortName, o => o.MapFrom(s => s.FromPort.Name))
                .ForMember(d => d.FromPortCode,
                o => o.MapFrom(s => s.FromPort.Code))
                .ForMember(d => d.ToPortName, o => o.MapFrom(s => s.ToPort.Name))
                .ForMember(d => d.ToPortCode, o => o.MapFrom(s => s.ToPort.Code));
            CreateMap<CreateRouteRequest, DomainRoute>();
            CreateMap<UpdateRouteRequest, DomainRoute>();

            // ── PricingEngine ────────────────────────────────────────────────
            CreateMap<Rate, RateResponse>()
                .ForMember(d => d.CarrierName, o => o.MapFrom(s => s.Carrier.Name))
                .ForMember(d => d.FromPortCode, o => o.MapFrom(s => s.Route.FromPort.Code))
                .ForMember(d => d.ToPortCode, o => o.MapFrom(s => s.Route.ToPort.Code))
                .ForMember(d => d.ContainerTypeName, o => o.MapFrom(s => s.ContainerType.Name));
            CreateMap<CreateRateRequest, Rate>();
            CreateMap<UpdateRateRequest, Rate>();

            // ── Quotation ────────────────────────────────────────────────────
            CreateMap<Quote, QuoteResponse>()
                .ForMember(d => d.FromPortCode, o => o.MapFrom(s => s.Route.FromPort.Code))
                .ForMember(d => d.CustomerName,
        o => o.MapFrom(s => s.Customer.ApplicationUser.FirstName + " " + s.Customer.ApplicationUser.LastName))
                .ForMember(d => d.ToPortCode, o => o.MapFrom(s => s.Route.ToPort.Code))
                .ForMember(d => d.ContainerTypeName,
                o => o.MapFrom(s => s.ContainerType.Name));
            CreateMap<CreateQuoteRequest, Quote>();
            CreateMap<UpdateQuoteRequest, Quote>();

            CreateMap<QuoteItem, QuoteItemResponse>();
            CreateMap<CreateQuoteItemRequest, QuoteItem>();
            CreateMap<UpdateQuoteItemRequest, QuoteItem>();

            // ── User ────────────────────────────────────────────────────
            CreateMap<ApplicationUser, AuthResponse>();
            CreateMap<LoginRequest, ApplicationUser>();
            CreateMap<RegisterRequest, ApplicationUser>();


            // ── Customer ────────────────────────────────────────────────────
            CreateMap<Customer, CustomerResponse>()
                .ForMember(d => d.Shipments, o => o.MapFrom(s => s.Shipments))
                .ForMember(d => d.Quotes, o => o.MapFrom(s => s.Quotes));
            CreateMap<CreateCustomerRequest, Customer>();
            CreateMap<UpdateCustomerRequest, Customer>();

            // ── Shipments ────────────────────────────────────────────────────
            CreateMap<Shipment, ShipmentResponse>()
                .ForMember(d => d.CarrierName, o => o.MapFrom(s => s.Carrier.Name))
                .ForMember(d => d.CustomerName, 
                o => o.MapFrom(s => s.Customer.ApplicationUser.FirstName +
                " " + s.Customer.ApplicationUser.LastName))
                .ForMember(d => d.ContainerTypeName,
                o => o.MapFrom(s => s.ContainerType.Name));
            CreateMap<CreateShipmentRequest, Shipment>();
            CreateMap<UpdateShipmentRequest, Shipment>();

            CreateMap<ShipmentCharge, ShipmentChargeResponse>();
            CreateMap<CreateShipmentChargeRequest, ShipmentCharge>();
            CreateMap<UpdateShipmentChargeRequest, ShipmentCharge>();

            CreateMap<ShipmentItem, ShipmentItemResponse>();
            CreateMap<CreateShipmentItemRequest, ShipmentItem>();
            CreateMap<UpdateShipmentItemRequest, ShipmentItem>();
            CreateMap<ShipmentStatusHistory, ShipmentStatusHistoryResponse>()
                .ForMember(d => d.ToStatus,
                o => o.MapFrom(s => s.ToStatus.ToString()))
                .ForMember(d => d.FromStatus,
                o => o.MapFrom(s => s.FromStatus.ToString()));


            // ── Profile ────────────────────────────────────────────────────
            CreateMap<ApplicationUser, ProfileResponse>()
                .ForMember(d => d.Name, o => o.MapFrom(s => $"{s.FirstName} {s.LastName}".Trim()))
                .ForMember(d => d.Customer, o => o.MapFrom(s => s.CustomerProfile));
            CreateMap<UpdateProfileRequest, ApplicationUser>();
            CreateMap<UpdatePasswordRequest, ApplicationUser>();


            // ── Invoice ────────────────────────────────────────────────────
            CreateMap<Invoice, InvoiceResponse>();
            CreateMap<CreateInvoiceRequest, Invoice>();

            // ── Shipment Documents ─────────────────────────────────────
            CreateMap<ShipmentDocument, ShipmentDocumentResponse>()
                .ForMember(d => d.Type,
                    o => o.MapFrom(s => s.Type.ToString()));

            CreateMap<IntegrationMessage, IntegrationMessageResponse>()
                .ForMember(d => d.Source,
                    o => o.MapFrom(s => s.Source.ToString()))
                .ForMember(d => d.ProcessingStatus,
                    o => o.MapFrom(s => s.ProcessingStatus.ToString()));
        }
    }
}