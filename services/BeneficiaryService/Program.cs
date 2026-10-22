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
using BeneficiaryService.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.ConfigureSerilog("BeneficiaryService");

// Configure OpenTelemetry Tracing & Metrics
builder.Services.AddCustomTelemetry("BeneficiaryService");

// Configure MassTransit
builder.Services.AddCustomMassTransit(builder.Configuration);

// Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? 
                       "Host=localhost;Database=EnterpriseBankDB;Username=postgres;Password=postgres";
}

builder.Services.AddDbContext<BeneficiaryDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<BeneficiaryDbContext>("BeneficiaryDatabase");

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Configure Middlewares
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

// Ensure Database schema exists on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<BeneficiaryDbContext>();
        context.Database.EnsureCreated();
        logger.LogInformation("Beneficiary schema and tables verified/created successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while creating/verifying the database.");
    }
}

app.Run();

public partial class Program { }
