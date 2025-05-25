using System.Net;
using BalanceManagementApiClient;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using MongoDB.Driver;
using PaymentIntegration.API.Middlewares;
using PaymentIntegration.API.Models;
using PaymentIntegration.Application.Interfaces;
using PaymentIntegration.Application.Validators;
using PaymentIntegration.Infrastructure.Data;
using PaymentIntegration.Infrastructure.Mappings;
using PaymentIntegration.Infrastructure.Migrations;
using PaymentIntegration.Infrastructure.Repositories;
using PaymentIntegration.Infrastructure.Services;
using Polly;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(builder.Configuration["MongoDB:ConnectionString"]));

builder.Services.AddHealthChecks()
    .AddMongoDb(
        clientFactory: sp => sp.GetRequiredService<IMongoClient>(),
        databaseNameFactory: _ => builder.Configuration["MongoDB:DatabaseName"],
        name: "mongodb",
        tags: new[] { "readiness" },
        timeout: TimeSpan.FromSeconds(5));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddTransient<MongoMigrationRunner>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();   

builder.Services.AddValidatorsFromAssemblyContaining<CreatePreOrderRequestValidator>();

var logger = LoggerFactory
    .Create(logging =>
    {
        logging.AddConsole();
    })
    .CreateLogger("PreAppLogger");

builder.Services.AddBalanceManagementApiClient(builder.Configuration["BalanceManagementAPI"],
    client =>
    {
        client.AddPolicyHandler(Policy
          //.Handle<ApiException>()
          //.Handle<ApiException>(ex => ex.StatusCode == 500 || ex.StatusCode == 400)
          .HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.InternalServerError)
          .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(2),
        onRetry: (outcome, timespan, retryAttempt, context) =>
        {
            
            logger.LogInformation(
                $"Retry attempt {retryAttempt} after {timespan.TotalSeconds}s due to HTTP 500 from {outcome.Result?.RequestMessage?.RequestUri}"
            );

        }));
    });

builder.Services.AddAutoMapper(typeof(PaymentMappingProfile).Assembly);
builder.Services.AddAutoMapper(typeof(ProductMappingProfile).Assembly);

builder.Services.AddMemoryCache();

var app = builder.Build();


app.MapOpenApi();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment Integration API");
    c.RoutePrefix = string.Empty;
});


using (var scope = app.Services.CreateScope())
{
    var migrations = new List<IMongoDbMigration>
    {
        new AddOrderIdIndexMigration(),
    };
    
    var runner = scope.ServiceProvider.GetRequiredService<MongoMigrationRunner>();
    await runner.RunMigrationsAsync(migrations);
}

// app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("readiness")
});


app.UseHttpMetrics();
app.MapMetrics();

app.UseMiddleware<HttpLoggingMiddleware>();
app.UseMiddleware<ValidationMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();