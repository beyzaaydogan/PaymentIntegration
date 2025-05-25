using PaymentIntegration.API.Models;

namespace PaymentIntegration.Application.Interfaces;

public interface IProductService
{
    Task<BaseResponse<List<ProductDto>>> GetAsync();
}