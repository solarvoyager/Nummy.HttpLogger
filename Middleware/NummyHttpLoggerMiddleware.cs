using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nummy.HttpLogger.Data.Services;
using Nummy.HttpLogger.Models;
using System.Diagnostics;

namespace Nummy.HttpLogger.Middleware;

internal class NummyHttpLoggerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly NummyHttpLoggerOptions _options;

    public NummyHttpLoggerMiddleware(RequestDelegate next, IOptions<NummyHttpLoggerOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Measure start time
        var startTime = Stopwatch.GetTimestamp();

        // Check exclusions
        var requestPath = context.Request.Path.Value;
        var isExcluded = _options.ExcludeContainingPaths
            .Any(path => requestPath.Contains(path, StringComparison.OrdinalIgnoreCase));

        if (isExcluded)
        {
            await _next(context);
            return;
        }

        // Create guid to don't wait for created id
        var httpLogGuid = Guid.NewGuid().ToString();

        // Log the request body
        // context.Request.ContentLength is > 0 && 
        if (_options.EnableRequestLogging)
            await ReadAndLogRequestBody(context, httpLogGuid);

        // Create memory stream to store response body
        var originalBody = context.Response.Body;
        await using var newMemoryStream = new MemoryStream();
        context.Response.Body = newMemoryStream;

        // Continue processing the request
        await _next(context);

        // Log the response body
        // context.Response.ContentLength is > 0 && 
        if (_options.EnableResponseLogging)
            await ReadAndLogResponseBody(context, originalBody, newMemoryStream, httpLogGuid);

        // Measure end time
        var endTime = Stopwatch.GetTimestamp();

        // Calculate response time in milliseconds
        var elapsedMs = (endTime - startTime) * 1000.0 / Stopwatch.Frequency;
    }

    private static async Task ReadAndLogResponseBody(HttpContext context, Stream originalBody,
        MemoryStream newMemoryStream, string httpLogGuid)
    {
        var service = context.RequestServices.GetRequiredService<INummyHttpLoggerService>();

        newMemoryStream.Seek(0, SeekOrigin.Begin);

        var responseBodyText = await new StreamReader(newMemoryStream).ReadToEndAsync();
        await service.LogResponseAsync(responseBodyText, httpLogGuid);

        newMemoryStream.Seek(0, SeekOrigin.Begin);

        context.Response.Body = originalBody;
        await context.Response.Body.WriteAsync(newMemoryStream.ToArray());
    }

    private static async Task ReadAndLogRequestBody(HttpContext context, string httpLogGuid)
    {
        var service = context.RequestServices.GetRequiredService<INummyHttpLoggerService>();

        context.Request.EnableBuffering();

        var requestBodyStream = new MemoryStream();
        await context.Request.Body.CopyToAsync(requestBodyStream);

        requestBodyStream.Seek(0, SeekOrigin.Begin);

        var requestBodyText = await new StreamReader(requestBodyStream).ReadToEndAsync();
        var remoteIp = context.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress?.ToString();
        await service.LogRequestAsync(requestBodyText, context.Request.Method, context.Request.Path,
            remoteIp ?? "0.0.0.0", httpLogGuid);

        context.Request.Body.Seek(0, SeekOrigin.Begin);
    }
}