using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Nummy.HttpLogger.Models;

namespace Nummy.HttpLogger.Middleware;

public class NummyHttpLoggerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly NummyHttpLoggerOptions _options;

    public NummyHttpLoggerMiddleware(RequestDelegate next, IOptions<NummyHttpLoggerOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task Invoke(HttpContext context)
    {
        // Check exclusions
        var requestPath = context.Request.Path.Value;
        if (_options.ExcludeContainingPaths.Any(path => requestPath.Contains(path, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        // Log the request body
        if (context.Request.ContentLength.HasValue && context.Request.ContentLength > 0)
        {
            context.Request.EnableBuffering();
            var requestBodyStream = new MemoryStream();
            await context.Request.Body.CopyToAsync(requestBodyStream);
            requestBodyStream.Seek(0, SeekOrigin.Begin);
            var requestBodyText = new StreamReader(requestBodyStream).ReadToEnd();
            //_logger.LogInformation($"Request Body: {requestBodyText}");
            context.Request.Body.Seek(0, SeekOrigin.Begin);
        }

        // Continue processing the request
        await _next(context);

        // Log the response body
        if (context.Response.ContentLength.HasValue && context.Response.ContentLength > 0)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            //_logger.LogInformation($"Response Body: {responseBodyText}");
            context.Response.Body.Seek(0, SeekOrigin.Begin);
        }
    }
}