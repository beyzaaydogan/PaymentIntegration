using PaymentIntegration.Domain.Entities;
using PaymentIntegration.Domain.Enums;

namespace PaymentIntegration.Application.Interfaces;

public interface IPaymentRepository
{
    Task<string> CreateAsync(Payment payment);

    Task<bool> UpdatePaymentStatusAsync(string orderId, PaymentStatus status);
}