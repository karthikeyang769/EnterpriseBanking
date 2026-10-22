using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedInfrastructure.Logging;
using SharedInfrastructure.Telemetry;
using SharedInfrastructure.Messaging;
using SharedInfrastructure.Middleware;
using AuditService.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.ConfigureSerilog("AuditService");

// Configure OpenTelemetry Distributed Tracing & Metrics
builder.Services.AddCustomTelemetry("AuditService");

// Configure RabbitMQ messaging, scanning the assembly to register all audit consumers
builder.Services.AddCustomMassTransit(builder.Configuration, typeof(AuditService.Consumers.CustomerRegisteredAuditConsumer).Assembly);

// Configure Database Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? 
                       "Host=localhost;Database=EnterpriseBankDB;Username=postgres;Password=postgres";
}

builder.Services.AddDbContext<AuditDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();

// Configure Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AuditDbContext>("AuditDatabase");

var app = builder.Build();

// Configure Middlewares
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

// Ensure database and schema exist on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<AuditDbContext>();
        context.Database.EnsureCreated();
        logger.LogInformation("Audit schema and tables verified/created successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while creating/verifying the database.");
    }
}

app.Run();

public partial class Program { }
