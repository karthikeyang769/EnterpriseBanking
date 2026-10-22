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
using IdentityService.Persistence;
using IdentityService.Application;
using IdentityService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.ConfigureSerilog("IdentityService");

// Configure Distributed Tracing & Metrics
builder.Services.AddCustomTelemetry("IdentityService");

// Configure Redis Caching
builder.Services.AddCustomRedis(builder.Configuration);

// Configure RabbitMQ messaging
builder.Services.AddCustomMassTransit(builder.Configuration);

// Add Clean Architecture layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<IdentityDbContext>("IdentityDatabase");

var app = builder.Build();

app.Use(async (context, next) =>
{
    var logText = $"[{DateTime.UtcNow:HH:mm:ss}] {context.Request.Method} {context.Request.Path}{context.Request.QueryString}{Environment.NewLine}";
    try
    {
        await System.IO.File.AppendAllTextAsync(@"e:\smartbanking\incoming_requests.log", logText);
    }
    catch { }
    await next();
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure Database and schema are created
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<IdentityDbContext>();
        context.Database.EnsureCreated();
        logger.LogInformation("Identity schema and tables created/verified successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while creating/verifying the database.");
    }
}

app.Run();

public partial class Program { }
