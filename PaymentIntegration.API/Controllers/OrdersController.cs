using Microsoft.AspNetCore.Mvc;
using PaymentIntegration.API.Models;
using PaymentIntegration.Application.Interfaces;

namespace PaymentIntegration.API.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public OrdersController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePreOrderRequest request)
    {   
        var baseResponse = await _paymentService.CreatePaymentAsync(request);
        if (baseResponse.Success)
            return Ok(baseResponse);
        
        return BadRequest(baseResponse);
    }
    
    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Complete(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("Id must be provided");
        }
        var baseResponse = await _paymentService.CompletePaymentAsync(id);
        
        if (baseResponse.Success)
            return Ok(baseResponse);
        
        return BadRequest(baseResponse);
    }
}