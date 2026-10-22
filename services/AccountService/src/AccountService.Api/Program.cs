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
using AccountService.Persistence;
using AccountService.Application;
using AccountService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.ConfigureSerilog("AccountService");

// Configure OpenTelemetry Tracing & Metrics
builder.Services.AddCustomTelemetry("AccountService");

// Configure Redis Caching
builder.Services.AddCustomRedis(builder.Configuration);

// Configure RabbitMQ messaging
builder.Services.AddCustomMassTransit(builder.Configuration);

// Register Account service layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AccountDbContext>("AccountDatabase");

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
        var context = services.GetRequiredService<AccountDbContext>();
        context.Database.EnsureCreated();
        logger.LogInformation("Account schema and tables created/verified successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while creating/verifying the database.");
    }
}

app.Run();

public partial class Program { }
