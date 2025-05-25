using MongoDB.Driver;
using PaymentIntegration.Application.Interfaces;
using PaymentIntegration.Domain.Entities;
using PaymentIntegration.Domain.Enums;
using PaymentIntegration.Infrastructure.Data;

namespace PaymentIntegration.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly IMongoCollection<Payment> _collection;

    public PaymentRepository(MongoDbContext context)
    {
        _collection = context.Payments;
    }

    public async Task<string> CreateAsync(Payment payment)
    {
        await _collection.InsertOneAsync(payment);
        return payment.Id;
    }
    
    public async Task<bool> UpdatePaymentStatusAsync(string orderId, PaymentStatus status)
    {
        var filter = Builders<Payment>.Filter.Eq(p => p.OrderId, orderId);

        var payment = await _collection.Find(filter).FirstOrDefaultAsync();
    
        // if the payment is null, or it is completed no update will be performed
        if (payment == null || status == PaymentStatus.Completed)
            return false;
        
        var update = Builders<Payment>.Update
            .Set(p => p.Status, status)
            .Set(p => p.UpdatedAt, DateTime.UtcNow);

        var updateResult = await _collection.UpdateOneAsync(filter, update);

        return updateResult.ModifiedCount != 0;
    }
}