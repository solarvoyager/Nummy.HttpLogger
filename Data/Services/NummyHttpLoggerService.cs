using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Nummy.HttpLogger.Data.Entitites;
using Nummy.HttpLogger.Utils;

namespace Nummy.HttpLogger.Data.Services;

internal class NummyHttpLoggerService(
    IHttpClientFactory clientFactory,
    ILogger<NummyHttpLoggerService> logger) : INummyHttpLoggerService
{
    public async Task LogRequestAsync(NummyRequestLog requestLog)
    {
        try
        {
            var client = clientFactory.CreateClient(NummyConstants.ClientName);
            await client.PostAsJsonAsync(NummyConstants.RequestLogAddUrl, requestLog).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Failed to send request log to Nummy service");
        }
    }

    public async Task LogResponseAsync(NummyResponseLog responseLog)
    {
        try
        {
            var client = clientFactory.CreateClient(NummyConstants.ClientName);
            await client.PostAsJsonAsync(NummyConstants.ResponseLogAddUrl, responseLog).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Failed to send response log to Nummy service");
        }
    }
}
