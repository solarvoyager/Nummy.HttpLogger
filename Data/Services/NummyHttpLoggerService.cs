using System.Net.Http.Json;
using Nummy.HttpLogger.Data.Entitites;
using Nummy.HttpLogger.Utils;

namespace Nummy.HttpLogger.Data.Services;

internal class NummyHttpLoggerService(IHttpClientFactory clientFactory) : INummyHttpLoggerService
{
    private readonly HttpClient _client = clientFactory.CreateClient(NummyConstants.ClientName);

    public async Task LogRequestAsync(NummyRequestLog requestLog)
    {
        await _client.PostAsJsonAsync(NummyConstants.RequestLogAddUrl, requestLog);
    }

    public async Task LogResponseAsync(NummyResponseLog responseLog)
    {
        await _client.PostAsJsonAsync(NummyConstants.ResponseLogAddUrl, responseLog);
    }
}