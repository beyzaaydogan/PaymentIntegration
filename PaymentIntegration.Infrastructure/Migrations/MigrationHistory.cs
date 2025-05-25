using MongoDB.Bson;

namespace PaymentIntegration.Infrastructure.Migrations;

public class MigrationHistory
{
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public string MigrationName { get; set; }
    public DateTime AppliedAt { get; set; }
}
