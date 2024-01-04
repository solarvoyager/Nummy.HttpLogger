using Nummy.HttpLogger.Data.Entitites;

namespace Nummy.HttpLogger.Data.Services;

internal interface INummyHttpLoggerService
{
    Task LogRequestAsync(NummyRequestLog requestLog);
    Task LogResponseAsync(NummyResponseLog responseLog);
}