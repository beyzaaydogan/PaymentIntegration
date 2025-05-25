using MongoDB.Driver;
using PaymentIntegration.Domain.Entities;

namespace PaymentIntegration.Infrastructure.Migrations;

public class AddOrderIdIndexMigration : IMongoDbMigration
{
    public string Name => "20240524_AddOrderIdIndex";

    public async Task UpAsync(IMongoDatabase database)
    {
        var payments = database.GetCollection<Payment>("Payments");

        var indexModel = new CreateIndexModel<Payment>(
            Builders<Payment>.IndexKeys.Ascending(p => p.OrderId),
            new CreateIndexOptions { Unique = true });

        await payments.Indexes.CreateOneAsync(indexModel);
    }
}
