using buildone.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace buildone.Middleware;

/// <summary>
/// Global exception handling middleware
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var requestId = Activity.Current?.Id ?? context.TraceIdentifier;
        
        // Log the exception with structured data
        _logger.LogError(exception,
            "Unhandled exception occurred. RequestId: {RequestId}, Path: {Path}, Method: {Method}",
            requestId,
            context.Request.Path,
            context.Request.Method);

        // Determine response based on exception type
        var response = exception switch
        {
            DbUpdateConcurrencyException => CreateResponse(
                HttpStatusCode.Conflict,
                "The record was modified by another user. Please refresh and try again.",
                requestId),
            
            DbUpdateException dbEx when IsUniqueConstraintViolation(dbEx) => CreateResponse(
                HttpStatusCode.Conflict,
                "A record with this information already exists.",
                requestId),
            
            UnauthorizedAccessException => CreateResponse(
                HttpStatusCode.Forbidden,
                "You do not have permission to perform this action.",
                requestId),
            
            KeyNotFoundException => CreateResponse(
                HttpStatusCode.NotFound,
                "The requested resource was not found.",
                requestId),
            
            ArgumentException argEx => CreateResponse(
                HttpStatusCode.BadRequest,
                argEx.Message,
                requestId),
            
            InvalidOperationException => CreateResponse(
                HttpStatusCode.BadRequest,
                "The operation is not valid in the current state.",
                requestId),
            
            _ => CreateResponse(
                HttpStatusCode.InternalServerError,
                _env.IsDevelopment() 
                    ? $"An error occurred: {exception.Message}" 
                    : "An unexpected error occurred. Please try again later.",
                requestId)
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)response.StatusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsJsonAsync(response.ApiResponse, jsonOptions);
    }

    private static (HttpStatusCode StatusCode, ApiResponse<object> ApiResponse) CreateResponse(
        HttpStatusCode statusCode,
        string message,
        string requestId)
    {
        var apiResponse = new ApiResponse<object>
        {
            Success = false,
            Message = message,
            RequestId = requestId,
            Timestamp = DateTime.UtcNow
        };

        return (statusCode, apiResponse);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        // Check for SQL Server unique constraint violation
        if (exception.InnerException is Microsoft.Data.SqlClient.SqlException sqlException)
        {
            // Error codes: 2627 (unique constraint), 2601 (unique index)
            return sqlException.Number == 2627 || sqlException.Number == 2601;
        }
        return false;
    }
}

/// <summary>
/// Extension method to register the middleware
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
