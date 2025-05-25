using PaymentIntegration.Domain.Enums;

namespace PaymentIntegration.Domain.Entities;

public class Payment
{
    public string Id { get; set; } = null!;

    public string OrderId { get; set; } = null!;
    
    public double Amount { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public PaymentStatus Status { get; set; }
}