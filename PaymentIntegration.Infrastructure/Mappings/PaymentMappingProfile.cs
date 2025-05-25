using AutoMapper;
using MongoDB.Bson;
using PaymentIntegration.API.Models;
using PaymentIntegration.Domain.Entities;
using PaymentIntegration.Domain.Enums;

namespace PaymentIntegration.Infrastructure.Mappings;

public class PaymentMappingProfile : Profile
{
    public PaymentMappingProfile()
    {
        CreateMap<CreatePreOrderRequest, Payment>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => ObjectId.GenerateNewId().ToString()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => PaymentStatus.Pending));

    }
}
