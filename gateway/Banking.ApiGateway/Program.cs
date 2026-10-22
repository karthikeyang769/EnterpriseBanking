using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using SharedInfrastructure.Logging;
using SharedInfrastructure.Middleware;
using SharedInfrastructure.Telemetry;

var builder = WebApplication.CreateBuilder(args);

// Configure Structured Serilog Logging
builder.ConfigureSerilog("Banking.ApiGateway");

// Configure OpenTelemetry Distributed Tracing & Metrics
builder.Services.AddCustomTelemetry("Banking.ApiGateway");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Configure Core Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("GatewayHealth", () => HealthCheckResult.Healthy("API Gateway is operational"));

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    var logText = $"[{DateTime.UtcNow:HH:mm:ss}] Gateway Received: {context.Request.Method} {context.Request.Path}{context.Request.QueryString}{Environment.NewLine}";
    try
    {
        await System.IO.File.AppendAllTextAsync(@"e:\smartbanking\gateway_requests.log", logText);
    }
    catch { }
    await next();
});

// Configure Middlewares
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger-docs/auth/v1/swagger.json", "Identity Service API v1");
    c.SwaggerEndpoint("/swagger-docs/customers/v1/swagger.json", "Customer Service API v1");
    c.SwaggerEndpoint("/swagger-docs/accounts/v1/swagger.json", "Account Service API v1");
    c.SwaggerEndpoint("/swagger-docs/transactions/v1/swagger.json", "Transaction Service API v1");
    c.SwaggerEndpoint("/swagger-docs/documents/v1/swagger.json", "Document Service API v1");
    c.SwaggerEndpoint("/swagger-docs/search/v1/swagger.json", "Search Service API v1");
    c.SwaggerEndpoint("/swagger-docs/beneficiary/v1/swagger.json", "Beneficiary Service API v1");
    c.SwaggerEndpoint("/swagger-docs/reporting/v1/swagger.json", "Reporting Service API v1");
    c.RoutePrefix = "swagger";
});

// Register Custom Infrastructural Middlewares
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("AllowAll");

// Map Unified Health Checks Endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            Status = report.Status.ToString(),
            Duration = report.TotalDuration,
            Details = report.Entries.Select(e => new
            {
                Key = e.Key,
                Status = e.Value.Status.ToString(),
                Description = e.Value.Description
            })
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
});

// Map YARP reverse proxy paths
app.MapReverseProxy();

app.Run();
public partial class Program { }
