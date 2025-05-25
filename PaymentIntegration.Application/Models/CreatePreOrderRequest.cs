namespace PaymentIntegration.API.Models;

public class CreatePreOrderRequest
{
    public string OrderId { get; set; }
    
    public double Amount { get; set; }
}