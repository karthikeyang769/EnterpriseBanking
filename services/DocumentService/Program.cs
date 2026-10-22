using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Minio;
using SharedInfrastructure.Logging;
using SharedInfrastructure.Telemetry;
using SharedInfrastructure.Messaging;
using SharedInfrastructure.Middleware;
using DocumentService.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.ConfigureSerilog("DocumentService");

// Configure OpenTelemetry Tracing & Metrics
builder.Services.AddCustomTelemetry("DocumentService");

// Configure MassTransit
builder.Services.AddCustomMassTransit(builder.Configuration);

// Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? 
                       "Host=localhost;Database=EnterpriseBankDB;Username=postgres;Password=postgres";
}

builder.Services.AddDbContext<DocumentDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register Minio Client
var minioHost = builder.Configuration["MinIO:Host"] ?? "localhost";
var minioPort = builder.Configuration["MinIO:Port"] ?? "9000";
var minioUser = builder.Configuration["MinIO:AccessKey"] ?? "admin";
var minioPass = builder.Configuration["MinIO:SecretKey"] ?? "admin123";

builder.Services.AddScoped<IMinioClient>(sp => new MinioClient()
    .WithEndpoint(minioHost, int.Parse(minioPort))
    .WithCredentials(minioUser, minioPass)
    .Build());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<DocumentDbContext>("DocumentDatabase");

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
        var context = services.GetRequiredService<DocumentDbContext>();
        context.Database.EnsureCreated();
        logger.LogInformation("Document schema and tables verified/created successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while creating/verifying the database.");
    }
}

app.Run();

public partial class Program { }
