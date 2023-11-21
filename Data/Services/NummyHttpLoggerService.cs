using Nummy.HttpLogger.Data.DataContext;
using Nummy.HttpLogger.Data.Entitites;

namespace Nummy.HttpLogger.Data.Services;

internal class NummyHttpLoggerService : INummyHttpLoggerService
{
    private readonly NummyHttpLoggerDataContext _nummyDataContext;

    public NummyHttpLoggerService(NummyHttpLoggerDataContext nummyDataContext)
    {
        _nummyDataContext = nummyDataContext;
    }

    public async Task LogRequestAsync(string requestBody, string requestMethod, string requestPath,
        string remoteIpAddress,
        string httpLogGuid)
    {
        var data = new NummyRequestLog
        {
            HttpLogGuid = httpLogGuid,
            CreatedAt = DateTimeOffset.Now,
            DeletedAt = null,
            IsDeleted = false,
            Body = requestBody,
            Method = requestMethod,
            Path = requestPath,
            RemoteIpAddress = remoteIpAddress
        };

        await _nummyDataContext.NummyRequestLogs.AddAsync(data);
        await _nummyDataContext.SaveChangesAsync();
    }

    public async Task LogResponseAsync(string responseBody, string httpLogGuid)
    {
        var data = new NummyResponseLog
        {
            HttpLogGuid = httpLogGuid,
            ResponseBody = responseBody
        };

        await _nummyDataContext.NummyResponseLogs.AddAsync(data);
        await _nummyDataContext.SaveChangesAsync();
    }
}