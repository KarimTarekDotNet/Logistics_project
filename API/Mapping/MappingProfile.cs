using Application.DTOs.ShippingCore;
using AutoMapper;
using DomainRoute = Domain.Entities.ShippingCore.Route;
using Domain.Entities.ShippingCore;
using Application.DTOs.Pricing.PricingEngine;
using Application.DTOs.Pricing.Quotation;
using Domain.Entities.Pricing.PricingEngine;
using Domain.Entities.Pricing.Quotation;
using Domain.Entities.Users;
using Application.DTOs.Auth;

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
        }
    }
}
