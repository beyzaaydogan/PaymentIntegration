using AutoMapper;
using PaymentIntegration.API.Models;
using PaymentIntegration.HttpClient;

namespace PaymentIntegration.Infrastructure.Mappings;

public class ProductMappingProfile :  Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDto>();
    }
}