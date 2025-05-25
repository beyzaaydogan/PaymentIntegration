using Microsoft.AspNetCore.Mvc;
using PaymentIntegration.Application.Interfaces;

namespace PaymentIntegration.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var baseResponse = await _productService.GetAsync();
        if (baseResponse.Success)
            return Ok(baseResponse);
        
        return BadRequest(baseResponse);
    }
}