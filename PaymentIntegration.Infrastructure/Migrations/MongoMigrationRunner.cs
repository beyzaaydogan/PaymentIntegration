using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using PaymentIntegration.Infrastructure.Data;

namespace PaymentIntegration.Infrastructure.Migrations;

public class MongoMigrationRunner
{
    private readonly MongoDbContext _context;
    private readonly ILogger<MongoMigrationRunner> _logger;
    private readonly IConfiguration _configuration; 

    public MongoMigrationRunner(MongoDbContext context, ILogger<MongoMigrationRunner> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task RunMigrationsAsync(IEnumerable<IMongoDbMigration> migrations)
    {
        var database = _context._client.GetDatabase(_configuration["MongoDB:DatabaseName"]);
        var migrationCollection = database.GetCollection<MigrationHistory>("MigrationHistory");
        var appliedMigrations = await migrationCollection.Find(_ => true)
            .Project(x => x.MigrationName)
            .ToListAsync();

        foreach (var migration in migrations)
        {
            if (appliedMigrations.Contains(migration.Name))
                continue;

            await migration.UpAsync(database);

            await migrationCollection.InsertOneAsync(new MigrationHistory
            {
                MigrationName = migration.Name,
                AppliedAt = DateTime.UtcNow
            });

            _logger.LogInformation($"Applied migration: {migration.Name}");
        }
    }
}
