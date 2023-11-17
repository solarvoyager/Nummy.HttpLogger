using Microsoft.Extensions.Options;

namespace Nummy.HttpLogger.Services
{
    internal class NummyHttpLogger : INummyHttpLogger
    {
        private readonly NummyLoggerOptions _options;

        public NummyHttpLogger(IOptions<NummyLoggerOptions> options)
        {
            _options = options.Value;
        }

        public void LogCustom(NummyLogLevel logLevel, string? title = null, string? description = null, Exception? exception = null)
        {
            throw new NotImplementedException();
        }

        public void LogDebug(string? title = null, string? description = null, Exception? exception = null)
        {
            throw new NotImplementedException();
        }

        public void LogError(string? title = null, string? description = null, Exception? exception = null)
        {
            throw new NotImplementedException();
        }

        public void LogInfo(string? title = null, string? description = null, Exception? exception = null)
        {
            throw new NotImplementedException();
        }
    }
}
