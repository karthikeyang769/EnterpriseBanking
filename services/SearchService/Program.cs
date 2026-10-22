using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nest;
using SharedInfrastructure.Logging;
using SharedInfrastructure.Telemetry;
using SharedInfrastructure.Messaging;
using SharedInfrastructure.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.ConfigureSerilog("SearchService");

// Configure OpenTelemetry Tracing & Metrics
builder.Services.AddCustomTelemetry("SearchService");

// Configure RabbitMQ messaging, registering our search consumers
builder.Services.AddCustomMassTransit(builder.Configuration, typeof(SearchService.Consumers.AccountCreatedSearchConsumer).Assembly);

// Register Elasticsearch NEST Client
var elasticUri = builder.Configuration["Elasticsearch:Uri"] ?? "http://localhost:9200";
var settings = new ConnectionSettings(new Uri(elasticUri))
    .DefaultIndex("accounts");
var client = new ElasticClient(settings);
builder.Services.AddSingleton<IElasticClient>(client);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Configure Middlewares
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();

public partial class Program { }
