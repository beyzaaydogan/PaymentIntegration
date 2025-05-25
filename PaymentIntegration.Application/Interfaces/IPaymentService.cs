using PaymentIntegration.API.Models;

namespace PaymentIntegration.Application.Interfaces;

public interface IPaymentService
{
    Task<BaseResponse<string>> CreatePaymentAsync(CreatePreOrderRequest createPreOrderRequest);
    
    Task<BaseResponse<string>> CompletePaymentAsync(string orderId);
}