using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedInfrastructure.Logging;
using SharedInfrastructure.Telemetry;
using SharedInfrastructure.Caching;
using SharedInfrastructure.Messaging;
using SharedInfrastructure.Middleware;
using TransactionService.Persistence;
using TransactionService.Application;
using TransactionService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.ConfigureSerilog("TransactionService");

// Configure OpenTelemetry Tracing & Metrics
builder.Services.AddCustomTelemetry("TransactionService");

// Configure Redis Cache
builder.Services.AddCustomRedis(builder.Configuration);

// Configure RabbitMQ messaging
builder.Services.AddCustomMassTransit(builder.Configuration);

// Register Transaction service layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<TransactionDbContext>("TransactionDatabase");

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthorization();
app.MapControllers();

// Ensure Database and schema are created on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<TransactionDbContext>();
        context.Database.EnsureCreated();
        logger.LogInformation("Transaction schema and tables created/verified successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while creating/verifying the database.");
    }
}

app.Run();

public partial class Program { }
