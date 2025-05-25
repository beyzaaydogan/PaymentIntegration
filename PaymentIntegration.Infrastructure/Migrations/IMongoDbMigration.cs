using MongoDB.Driver;

namespace PaymentIntegration.Infrastructure.Migrations;

public interface IMongoDbMigration
{
    string Name { get; }
    Task UpAsync(IMongoDatabase database);
}