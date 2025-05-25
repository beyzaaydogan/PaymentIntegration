using System.Text.Json;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PaymentIntegration.API.Models;
using PaymentIntegration.Application.Interfaces;
using PaymentIntegration.HttpClient;

namespace PaymentIntegration.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<ProductService> _logger;
    private readonly IBalanceManagementApiClient _balanceManagementApiClient;
    private IMapper _mapper;
    
    private const string ProductCacheKey = "productList";
    
    public ProductService(IMemoryCache cache, ILogger<ProductService> logger, IBalanceManagementApiClient balanceManagementApiClient, IMapper mapper)
    {
        _cache = cache;
        _logger = logger;
        _balanceManagementApiClient = balanceManagementApiClient;
        _mapper = mapper;
    }

    public async Task<BaseResponse<List<ProductDto>>> GetAsync()
    {
        if (_cache.TryGetValue(ProductCacheKey, out List<ProductDto> cachedProducts))
        {
            _logger.LogInformation("Get products from cache");
            return  BaseResponse<List<ProductDto>>.Ok(cachedProducts);
        }
        try
        {
            var response = await _balanceManagementApiClient.ProductsAsync();
            List<ProductDto> productList = _mapper.Map<List<ProductDto>>(response.Data);
            
            _cache.Set(ProductCacheKey, productList, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });
            
            return  BaseResponse<List<ProductDto>>.Ok(productList);

        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error: {JsonSerializer.Serialize(ex.Message)}");
            return BaseResponse<List<ProductDto>>.Fail(ex.Message);
        }
    }
}