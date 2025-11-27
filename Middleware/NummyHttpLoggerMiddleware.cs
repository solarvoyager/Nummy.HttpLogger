using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nummy.HttpLogger.Data.Entitites;
using Nummy.HttpLogger.Data.Services;
using Nummy.HttpLogger.Utils;

namespace Nummy.HttpLogger.Middleware;

internal sealed class NummyHttpLoggerMiddleware(RequestDelegate next, IOptions<NummyHttpLoggerOptions> options)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Exclusions (case-insensitive)
        var pathValue = context.Request.Path.HasValue ? context.Request.Path.Value : string.Empty;

        var isExcluded = options.Value.ExcludeContainingPaths
            .Any(p => !string.IsNullOrEmpty(p) &&
                      pathValue.Contains(p, StringComparison.OrdinalIgnoreCase));
        if (isExcluded)
        {
            await next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var httpLogId = Guid.NewGuid();

        // Log request (non-blocking pattern recommended — here we await but swallow errors)
        if (options.Value.EnableRequestLogging)
        {
            try
            {
                await ReadAndLogRequestBodyAsync(context, httpLogId, options.Value);
            }
            catch (Exception ex)
            {
                // Don't fail the request if logging fails. Consider ILogger here.
            }
        }

        var originalResponseBody = context.Response.Body;
        await using var bufferStream = new MemoryStream();
        context.Response.Body = bufferStream;

        try
        {
            // Execute next middleware
            await next(context);

            stopwatch.Stop();

            if (options.Value.EnableResponseLogging)
            {
                try
                {
                    await ReadAndLogResponseBodyAsync(
                        context,
                        originalResponseBody,
                        bufferStream,
                        httpLogId,
                        stopwatch.ElapsedMilliseconds,
                        options.Value
                    );
                }
                catch (Exception ex)
                {
                    // swallow; do not break response
                }
            }
            else
            {
                // If we're not logging response, just copy it back
                bufferStream.Seek(0, SeekOrigin.Begin);
                await bufferStream.CopyToAsync(originalResponseBody);
            }
        }
        finally
        {
            // Ensure Response.Body is always restored
            context.Response.Body = originalResponseBody;
        }
    }

    private static bool IsBinaryContentType(string? contentType)
    {
        if (string.IsNullOrEmpty(contentType)) return false;
        contentType = contentType.ToLowerInvariant();
        if (contentType.StartsWith("text/")) return false;
        if (contentType.Contains("json") || contentType.Contains("xml") || contentType.Contains("html")) return false;
        // common binary types
        return contentType.StartsWith("image/") ||
               contentType.StartsWith("video/") ||
               contentType.StartsWith("audio/") ||
               contentType.StartsWith("pdf/") ||
               contentType.Contains("octet-stream") ||
               contentType.Contains("application") ||
               contentType.Contains("multipart/");
    }

    private static string MaskHeaders(IHeaderDictionary headers, NummyHttpLoggerOptions options)
    {
        // Simple example: create a small string of headers and mask Authorization/Cookie
        var sb = new StringBuilder();
        foreach (var kv in headers)
        {
            var key = kv.Key;
            var val = kv.Value.ToString();

            var shouldMask = options.MaskHeaders.Contains(kv.Key);
            
            if (shouldMask)
            {
                val = "[MASKED]";
            }

            sb.AppendLine($"{key}: {val}");
        }

        return sb.ToString();
    }

    private static async Task ReadAndLogRequestBodyAsync(
        HttpContext context,
        Guid httpLogGuid,
        NummyHttpLoggerOptions options)
    {
        var service = context.RequestServices.GetRequiredService<INummyHttpLoggerService>();

        string? toLog = null;

        // if content is binary or empty, skip
        var ct = context.Request.ContentType;
        var isBinary = IsBinaryContentType(ct);

        if (!isBinary)
        {
            context.Request.EnableBuffering(); // spools to temp file when large

            // Read only up to MaxBodyLength
            var max = options.MaxBodyLength;
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false, bufferSize: 8192, leaveOpen: true);
            var full = await reader.ReadToEndAsync();

            toLog = full.Length > max ? full[..max] + "...(truncated)" : full;
        }
        
        // gather extra info
        var remoteIp = context.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress?.ToString();
        var headers = MaskHeaders(context.Request.Headers, options);

        // fire-and-forget is recommended; for now await but swallow exceptions
        try
        {
            await service.LogRequestAsync(new NummyRequestLog
            {
                Body = isBinary ? "[BINARY CONTENT]" : toLog,
                TraceIdentifier = context.TraceIdentifier,
                ApplicationId = options.ApplicationId,
                Method = context.Request.Method,
                HttpLogId = httpLogGuid,
                Path = context.Request.Path,
                RemoteIp = remoteIp,
                Headers = headers
            });
        }
        catch
        {
            // swallow
        }

        // rewind request body for downstream middleware
        context.Request.Body.Seek(0, SeekOrigin.Begin);
    }

    private static async Task ReadAndLogResponseBodyAsync(
        HttpContext context, 
        Stream originalBody,
        MemoryStream bufferStream,
        Guid httpLogGuid, 
        long elapsedMs,
        NummyHttpLoggerOptions options)
    {
        var service = context.RequestServices.GetRequiredService<INummyHttpLoggerService>();

        bufferStream.Seek(0, SeekOrigin.Begin);

        var ct = context.Response.ContentType;
        if (IsBinaryContentType(ct))
        {
            // Copy response back to original and optionally log only headers/status
            await bufferStream.CopyToAsync(originalBody);

            try
            {
                var headers = MaskHeaders(context.Response.Headers, options);
                await service.LogResponseAsync(new NummyResponseLog
                {
                    Body = "[BINARY CONTENT]",
                    HttpLogId = httpLogGuid,
                    StatusCode = context.Response.StatusCode,
                    DurationMs = elapsedMs,
                    Headers = headers
                });
            }
            catch
            {
                /* swallow */
            }

            return;
        }

        using var reader = new StreamReader(bufferStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);
        var fullBody = await reader.ReadToEndAsync();

        var max = options.MaxBodyLength;
        var toLog = fullBody.Length > max ? fullBody[..max] + "...(truncated)" : fullBody;

        // Important: rewind before copying to the real response stream
        bufferStream.Seek(0, SeekOrigin.Begin);
        await bufferStream.CopyToAsync(originalBody);

        try
        {
            var headers = MaskHeaders(context.Response.Headers, options);
            await service.LogResponseAsync(new NummyResponseLog
            {
                Body = toLog,
                HttpLogId = httpLogGuid,
                StatusCode = context.Response.StatusCode,
                DurationMs = elapsedMs,
                Headers = headers
            });
        }
        catch
        {
            // swallow
        }
    }
}