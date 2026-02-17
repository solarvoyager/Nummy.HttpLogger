using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nummy.HttpLogger.Data.Entitites;
using Nummy.HttpLogger.Data.Services;
using Nummy.HttpLogger.Utils;

namespace Nummy.HttpLogger.Middleware;

internal sealed class NummyHttpLoggerMiddleware(
    RequestDelegate next,
    IOptions<NummyHttpLoggerOptions> options,
    ILogger<NummyHttpLoggerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
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

        if (options.Value.EnableRequestLogging)
        {
            try
            {
                await ReadAndLogRequestBodyAsync(context, httpLogId, options.Value);
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "Failed to log HTTP request to Nummy service");
            }
        }

        if (options.Value.EnableResponseLogging)
        {
            var originalResponseBody = context.Response.Body;
            await using var bufferStream = new MemoryStream();
            context.Response.Body = bufferStream;

            try
            {
                await next(context);

                stopwatch.Stop();

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
                    logger.LogDebug(ex, "Failed to log HTTP response to Nummy service");
                    bufferStream.Seek(0, SeekOrigin.Begin);
                    await bufferStream.CopyToAsync(originalResponseBody);
                }
            }
            finally
            {
                context.Response.Body = originalResponseBody;
            }
        }
        else
        {
            await next(context);
            stopwatch.Stop();
        }
    }

    private static bool IsBinaryContentType(string? contentType)
    {
        if (string.IsNullOrEmpty(contentType)) return false;
        contentType = contentType.ToLowerInvariant();
        if (contentType.StartsWith("text/")) return false;
        if (contentType.Contains("json") || contentType.Contains("xml") ||
            contentType.Contains("html") || contentType.Contains("form-urlencoded")) return false;
        return true;
    }

    private static List<NummyHeader> MaskHeaders(IHeaderDictionary headers, NummyHttpLoggerOptions options)
    {
        var copiedHeaders = new List<NummyHeader>();
        foreach (var (key, value) in headers)
        {
            var val = value.ToString();

            var shouldMask = options.MaskHeaders.Contains(key);

            if (shouldMask)
            {
                val = "[MASKED]";
            }

            copiedHeaders.Add(new NummyHeader
            {
                Key = key,
                Value = val
            });
        }

        return copiedHeaders;
    }

    private static async Task ReadAndLogRequestBodyAsync(
        HttpContext context,
        Guid httpLogGuid,
        NummyHttpLoggerOptions options)
    {
        var service = context.RequestServices.GetRequiredService<INummyHttpLoggerService>();

        string? toLog = null;

        var ct = context.Request.ContentType;
        var isBinary = IsBinaryContentType(ct);

        if (!isBinary)
        {
            context.Request.EnableBuffering();

            var max = options.MaxBodyLength;
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false, bufferSize: 8192, leaveOpen: true);

            var buffer = new char[max + 1];
            var charsRead = await reader.ReadBlockAsync(buffer, 0, buffer.Length);

            toLog = charsRead > max
                ? new string(buffer, 0, max) + "...(truncated)"
                : new string(buffer, 0, charsRead);
        }

        var remoteIp = context.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress?.ToString();
        var headers = MaskHeaders(context.Request.Headers, options);

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
            // swallow — logged at middleware level
        }

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
                // swallow — logged at middleware level
            }

            return;
        }

        var max = options.MaxBodyLength;
        string toLog;

        using (var reader = new StreamReader(bufferStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false,
                   leaveOpen: true))
        {
            var buffer = new char[max + 1];
            var charsRead = await reader.ReadBlockAsync(buffer, 0, buffer.Length);

            toLog = charsRead > max
                ? new string(buffer, 0, max) + "...(truncated)"
                : new string(buffer, 0, charsRead);
        }

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
            // swallow — logged at middleware level
        }
    }
}
