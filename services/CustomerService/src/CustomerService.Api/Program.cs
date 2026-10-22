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
using CustomerService.Persistence;
using CustomerService.Application;
using CustomerService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.ConfigureSerilog("CustomerService");

// Configure OpenTelemetry Tracing & Metrics
builder.Services.AddCustomTelemetry("CustomerService");

// Configure Redis Cache
builder.Services.AddCustomRedis(builder.Configuration);

// Configure RabbitMQ messaging, registering consumers from the Application assembly
builder.Services.AddCustomMassTransit(builder.Configuration, typeof(CustomerService.Application.Consumers.CustomerRegisteredConsumer).Assembly);

// Add Clean Architecture layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<CustomerDbContext>("CustomerDatabase");

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
        var context = services.GetRequiredService<CustomerDbContext>();
        context.Database.EnsureCreated();
        logger.LogInformation("Customer schema and tables created/verified successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while creating/verifying the database.");
    }
}

app.Run();

public partial class Program { }
