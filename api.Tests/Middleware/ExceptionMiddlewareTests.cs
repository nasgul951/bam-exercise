using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using StargateAPI.Business.Exceptions;
using StargateAPI.Controllers;
using StargateAPI.Middleware;

namespace StargateAPI.Tests.Middleware;

public class ExceptionMiddlewareTests
{
    private static (ExceptionMiddleware Middleware, DefaultHttpContext HttpContext, MemoryStream Body)
        CreateSut(Mock<RequestDelegate> nextMock, Mock<ILogger<ExceptionMiddleware>>? loggerMock = null)
    {
        loggerMock ??= new Mock<ILogger<ExceptionMiddleware>>();
        var middleware = new ExceptionMiddleware(nextMock.Object, loggerMock.Object);
        var httpContext = new DefaultHttpContext();
        var body = new MemoryStream();
        httpContext.Response.Body = body;
        return (middleware, httpContext, body);
    }

    private static async Task<BaseResponse?> ReadResponse(MemoryStream body)
    {
        body.Seek(0, SeekOrigin.Begin);
        return await JsonSerializer.DeserializeAsync<BaseResponse>(body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    [Fact]
    public async Task InvokeAsync_WhenBadHttpRequestExceptionThrown_Returns400WithMessage()
    {
        var next = new Mock<RequestDelegate>();
        next.Setup(n => n(It.IsAny<HttpContext>()))
            .ThrowsAsync(new BadHttpRequestException("bad input"));
        var (middleware, ctx, body) = CreateSut(next);

        await middleware.InvokeAsync(ctx);

        Assert.Equal(StatusCodes.Status400BadRequest, ctx.Response.StatusCode);
        var response = await ReadResponse(body);
        Assert.NotNull(response);
        Assert.Equal("bad input", response.Message);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task InvokeAsync_WhenNotFoundExceptionThrown_Returns404WithMessage()
    {
        var next = new Mock<RequestDelegate>();
        next.Setup(n => n(It.IsAny<HttpContext>()))
            .ThrowsAsync(new NotFoundException("not found"));
        var (middleware, ctx, body) = CreateSut(next);

        await middleware.InvokeAsync(ctx);

        Assert.Equal(StatusCodes.Status404NotFound, ctx.Response.StatusCode);
        var response = await ReadResponse(body);
        Assert.NotNull(response);
        Assert.Equal("not found", response.Message);
    }

    [Fact]
    public async Task InvokeAsync_WhenGenericExceptionThrown_Returns500WithGenericMessage()
    {
        var next = new Mock<RequestDelegate>();
        next.Setup(n => n(It.IsAny<HttpContext>()))
            .ThrowsAsync(new InvalidOperationException("secret internal detail"));
        var (middleware, ctx, body) = CreateSut(next);

        await middleware.InvokeAsync(ctx);

        Assert.Equal(StatusCodes.Status500InternalServerError, ctx.Response.StatusCode);
        var response = await ReadResponse(body);
        Assert.NotNull(response);
        Assert.Equal("Internal Server Error", response.Message);
        Assert.DoesNotContain("secret internal detail", response.Message);
    }

    [Fact]
    public async Task InvokeAsync_WhenGenericExceptionThrown_LogsError()
    {
        var next = new Mock<RequestDelegate>();
        var ex = new InvalidOperationException("oops");
        next.Setup(n => n(It.IsAny<HttpContext>())).ThrowsAsync(ex);
        var loggerMock = new Mock<ILogger<ExceptionMiddleware>>();
        var (middleware, ctx, _) = CreateSut(next, loggerMock);

        await middleware.InvokeAsync(ctx);

        loggerMock.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            ex,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenNoExceptionThrown_PassesThrough()
    {
        var called = false;
        var next = new Mock<RequestDelegate>();
        next.Setup(n => n(It.IsAny<HttpContext>()))
            .Returns<HttpContext>(_ => { called = true; return Task.CompletedTask; });
        var (middleware, ctx, _) = CreateSut(next);

        await middleware.InvokeAsync(ctx);

        Assert.True(called);
        Assert.Equal(200, ctx.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_SetsContentTypeToApplicationJson()
    {
        var next = new Mock<RequestDelegate>();
        next.Setup(n => n(It.IsAny<HttpContext>()))
            .ThrowsAsync(new BadHttpRequestException("bad"));
        var (middleware, ctx, _) = CreateSut(next);

        await middleware.InvokeAsync(ctx);

        Assert.StartsWith("application/json", ctx.Response.ContentType);
    }
}
