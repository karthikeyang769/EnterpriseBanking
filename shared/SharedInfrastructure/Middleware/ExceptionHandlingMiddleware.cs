using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SharedContracts.Responses;
using SharedKernel.Domain;

namespace SharedInfrastructure.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var statusCode = HttpStatusCode.InternalServerError;
        var response = new ApiResponse { Success = false };

        var correlationId = context.Items.TryGetValue("X-Correlation-ID", out var value) ? value?.ToString() : string.Empty;
        response.TraceId = correlationId ?? context.TraceIdentifier;

        // Use reflection or type check for FluentValidation exception to prevent tight coupling if it is not loaded,
        // but here we check it directly since we will ensure the package is added.
        if (exception is DomainException domainException)
        {
            statusCode = HttpStatusCode.BadRequest;
            response.Message = domainException.Message;
            response.Errors = new List<ErrorModel>
            {
                new("Domain", domainException.Message, domainException.ErrorCode)
            };
        }
        else if (exception.GetType().FullName == "FluentValidation.ValidationException")
        {
            statusCode = HttpStatusCode.BadRequest;
            response.Message = "Validation failed.";
            response.Errors = new List<ErrorModel>();
            
            // Dynamic resolution of FluentValidation properties to avoid strict compile-time errors if fluent validation isn't loaded
            dynamic valEx = exception;
            if (valEx.Errors != null)
            {
                foreach (var err in valEx.Errors)
                {
                    response.Errors.Add(new ErrorModel(err.PropertyName, err.ErrorMessage, err.ErrorCode ?? "VALIDATION_ERROR"));
                }
            }
        }
        else
        {
            statusCode = HttpStatusCode.InternalServerError;
            response.Message = "An unexpected error occurred.";
            response.Errors = new List<ErrorModel>
            {
                new("Server", exception.Message, "INTERNAL_SERVER_ERROR")
            };
        }

        context.Response.StatusCode = (int)statusCode;
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false 
        });
        
        return context.Response.WriteAsync(json);
    }
}
