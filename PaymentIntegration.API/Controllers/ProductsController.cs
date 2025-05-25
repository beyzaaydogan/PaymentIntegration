using Microsoft.AspNetCore.Mvc;
using PaymentIntegration.API.Models;
using PaymentIntegration.Application.Interfaces;

namespace PaymentIntegration.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(BaseResponse<List<ProductDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<List<ProductDto>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get()
    {
        var baseResponse = await _productService.GetAsync();
        if (baseResponse.Success)
            return Ok(baseResponse);
        
        return BadRequest(baseResponse);
    }
}