using StargateAPI.Business.Common;
using StargateAPI.Business.Data;
using StargateAPI.Business.Exceptions;
using StargateAPI.Controllers;

namespace StargateAPI.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
            await HandleExceptionAsync(context, ex, _logger);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex, ILogger logger)
    {
        var statusCode = ex switch
        {
            BadHttpRequestException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        string message = String.Empty;
        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(ex, "Internal Server Error");
            message = "Internal Server Error";
        }
        else
        {
            message = ex.Message;
        }

        try
        {
            var db = context.RequestServices.GetService<StargateContext>();
            if (db is not null)
            {
                var level = statusCode == StatusCodes.Status500InternalServerError ? LogLevels.Error : LogLevels.Warning;
                db.AddLog(level, nameof(ExceptionMiddleware), message ?? string.Empty, ex);
                await db.SaveChangesAsync();
            }
        }
        catch { /* swallow — never let logging failure break error handling */ }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var problem = new BaseResponse()
        {
            Message = message,
            Success = false,
            ResponseCode = statusCode
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
}
