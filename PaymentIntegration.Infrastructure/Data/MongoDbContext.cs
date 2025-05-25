using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using PaymentIntegration.Domain.Entities;

namespace PaymentIntegration.Infrastructure.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    public readonly IMongoClient _client;

    public MongoDbContext(IConfiguration configuration)
    {
        _client = new MongoClient(configuration["MongoDB:ConnectionString"]);
        _database = _client.GetDatabase(configuration["MongoDB:DatabaseName"]);
    }

    public IMongoCollection<Payment> Payments => _database.GetCollection<Payment>("Payments");

}