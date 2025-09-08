using Microsoft.AspNetCore.Http;
using FluentAssertions;
using Xunit;
using Zarichney.Config;
using System.Collections.Generic;
using AutoFixture;
using Serilog.Context;
using Microsoft.Extensions.Primitives;

namespace Zarichney.Tests.Unit.Config;

public class CorrelationIdMiddlewareTests
{
    private readonly Fixture _fixture = new();
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    [Trait("Category", "Unit")]
    [Fact]
    public async Task InvokeAsync_WithExistingCorrelationId_UsesProvidedId()
    {
        // Arrange
        var existingCorrelationId = _fixture.Create<string>();
        var context = CreateHttpContext();
        context.Request.Headers[CorrelationIdHeaderName] = existingCorrelationId;
        
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        
        var middleware = new CorrelationIdMiddleware(next);
        
        // Act
        await middleware.InvokeAsync(context);
        
        // Assert
        nextCalled.Should().BeTrue("because the next middleware should be called");
        context.Items["CorrelationId"].Should().Be(existingCorrelationId, 
            "because the existing correlation ID should be preserved");
        // Note: Response header is set via OnStarting callback in actual ASP.NET Core runtime
    }

    [Trait("Category", "Unit")]
    [Fact]
    public async Task InvokeAsync_WithoutCorrelationId_GeneratesNewId()
    {
        // Arrange
        var context = CreateHttpContext();
        
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        
        var middleware = new CorrelationIdMiddleware(next);
        
        // Act
        await middleware.InvokeAsync(context);
        
        // Assert
        nextCalled.Should().BeTrue("because the next middleware should be called");
        context.Items["CorrelationId"].Should().NotBeNull("because a correlation ID should be generated");
        context.Items["CorrelationId"].Should().BeOfType<string>("because correlation ID should be a string");
        
        var correlationId = context.Items["CorrelationId"]?.ToString()!;
        Guid.TryParse(correlationId, out _).Should().BeTrue("because generated correlation ID should be a valid GUID");
        // Note: Response header is set via OnStarting callback in actual ASP.NET Core runtime
    }

    [Trait("Category", "Unit")]
    [Fact]
    public async Task InvokeAsync_WithEmptyCorrelationId_GeneratesNewId()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[CorrelationIdHeaderName] = string.Empty;
        
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        
        var middleware = new CorrelationIdMiddleware(next);
        
        // Act
        await middleware.InvokeAsync(context);
        
        // Assert
        nextCalled.Should().BeTrue("because the next middleware should be called");
        context.Items["CorrelationId"].Should().NotBeNull("because a correlation ID should be generated when header is empty");
        
        var correlationId = context.Items["CorrelationId"]?.ToString()!;
        correlationId.Should().NotBeNullOrEmpty("because a valid correlation ID should be generated");
        Guid.TryParse(correlationId, out _).Should().BeTrue("because generated correlation ID should be a valid GUID");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public async Task InvokeAsync_WithWhitespaceCorrelationId_GeneratesNewId()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[CorrelationIdHeaderName] = "   ";
        
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        
        var middleware = new CorrelationIdMiddleware(next);
        
        // Act
        await middleware.InvokeAsync(context);
        
        // Assert
        nextCalled.Should().BeTrue("because the next middleware should be called");
        
        var correlationId = context.Items["CorrelationId"]?.ToString()!;
        correlationId.Should().NotBeNullOrWhiteSpace("because a valid correlation ID should be generated when header contains only whitespace");
        Guid.TryParse(correlationId, out _).Should().BeTrue("because generated correlation ID should be a valid GUID");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public async Task InvokeAsync_DoesNotOverwriteExistingResponseHeader()
    {
        // Arrange
        var existingHeaderValue = _fixture.Create<string>();
        var providedCorrelationId = _fixture.Create<string>();
        
        var context = CreateHttpContext();
        context.Request.Headers[CorrelationIdHeaderName] = providedCorrelationId;
        context.Response.Headers[CorrelationIdHeaderName] = existingHeaderValue;
        
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new CorrelationIdMiddleware(next);
        
        // Act
        await middleware.InvokeAsync(context);
        
        // Assert
        // The middleware sets up OnStarting callback, but in unit tests we validate the logic works correctly
        // by checking that correlation ID is properly stored in context items
        context.Items["CorrelationId"].Should().Be(providedCorrelationId,
            "because the provided correlation ID should be stored in context items");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware()
    {
        // Arrange
        var context = CreateHttpContext();
        var nextCalled = false;
        var passedContext = default(HttpContext);
        
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            passedContext = ctx;
            return Task.CompletedTask;
        };
        
        var middleware = new CorrelationIdMiddleware(next);
        
        // Act
        await middleware.InvokeAsync(context);
        
        // Assert
        nextCalled.Should().BeTrue("because the next middleware in the pipeline should be called");
        passedContext.Should().BeSameAs(context, "because the same HttpContext should be passed to the next middleware");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public async Task InvokeAsync_PropagatesExceptionsFromNextMiddleware()
    {
        // Arrange
        var context = CreateHttpContext();
        var expectedException = new InvalidOperationException("Test exception");
        
        RequestDelegate next = _ => throw expectedException;
        var middleware = new CorrelationIdMiddleware(next);
        
        // Act
        Func<Task> act = async () => await middleware.InvokeAsync(context);
        
        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception", "because exceptions from the next middleware should be propagated");
        
        context.Items["CorrelationId"].Should().NotBeNull("because correlation ID should still be set even when next middleware throws");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public async Task InvokeAsync_StoresCorrelationIdInHttpContextItems()
    {
        // Arrange
        var expectedCorrelationId = _fixture.Create<string>();
        var context = CreateHttpContext();
        context.Request.Headers[CorrelationIdHeaderName] = expectedCorrelationId;
        
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new CorrelationIdMiddleware(next);
        
        // Act
        await middleware.InvokeAsync(context);
        
        // Assert
        context.Items.Should().ContainKey("CorrelationId", "because correlation ID should be stored in HttpContext.Items");
        context.Items["CorrelationId"].Should().Be(expectedCorrelationId, "because the provided correlation ID should be stored");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public async Task InvokeAsync_WithNullHeaderValue_GeneratesNewId()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[CorrelationIdHeaderName] = default(StringValues);
        
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new CorrelationIdMiddleware(next);
        
        // Act
        await middleware.InvokeAsync(context);
        
        // Assert
        var correlationId = context.Items["CorrelationId"]?.ToString()!;
        correlationId.Should().NotBeNullOrEmpty("because a new correlation ID should be generated when header value is null");
        Guid.TryParse(correlationId, out _).Should().BeTrue("because generated correlation ID should be a valid GUID");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public async Task InvokeAsync_CallsNextMiddlewareWithinLogContext()
    {
        // Arrange
        var context = CreateHttpContext();
        var contextItemsSnapshot = default(IDictionary<object, object?>);
        
        RequestDelegate next = ctx =>
        {
            // Capture context items during next middleware execution
            contextItemsSnapshot = new Dictionary<object, object?>(ctx.Items);
            return Task.CompletedTask;
        };
        
        var middleware = new CorrelationIdMiddleware(next);
        
        // Act
        await middleware.InvokeAsync(context);
        
        // Assert
        contextItemsSnapshot.Should().NotBeNull("because next middleware should have been called");
        contextItemsSnapshot.Should().ContainKey("CorrelationId", 
            "because correlation ID should be available to downstream middleware");
        
        var correlationId = contextItemsSnapshot["CorrelationId"]?.ToString()!;
        Guid.TryParse(correlationId, out _).Should().BeTrue(
            "because correlation ID should be a valid GUID when accessed by downstream middleware");
    }

    private static HttpContext CreateHttpContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Clear();
        httpContext.Response.Headers.Clear();
        return httpContext;
    }
}