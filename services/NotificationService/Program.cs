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
using NotificationService.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.ConfigureSerilog("NotificationService");

// Configure OpenTelemetry Distributed Tracing & Metrics
builder.Services.AddCustomTelemetry("NotificationService");

// Configure RabbitMQ messaging with consumers
builder.Services.AddCustomMassTransit(builder.Configuration, typeof(NotificationService.Consumers.CustomerRegisteredNotificationConsumer).Assembly);

// Configure Database Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? 
                       "Host=localhost;Database=EnterpriseBankDB;Username=postgres;Password=postgres";
}

builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();

// Configure Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<NotificationDbContext>("NotificationDatabase");

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
        var context = services.GetRequiredService<NotificationDbContext>();
        context.Database.EnsureCreated();
        logger.LogInformation("Notification schema and tables verified/created successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while creating/verifying the database.");
    }
}

app.Run();

public partial class Program { }
