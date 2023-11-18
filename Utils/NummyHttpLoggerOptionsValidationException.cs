using Nummy.HttpLogger.Models;

namespace Nummy.HttpLogger.Utils;

internal class NummyHttpLoggerOptionsValidationException : Exception
{
    public NummyHttpLoggerOptionsValidationException()
        : base($"{nameof(NummyHttpLoggerOptions.DatabaseConnectionString)} must have a valid connection string")
    {
    }
}