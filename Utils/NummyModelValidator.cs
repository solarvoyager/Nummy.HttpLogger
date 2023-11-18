using Nummy.HttpLogger.Models;

namespace Nummy.HttpLogger.Utils;

public static class NummyModelValidator
{
    public static void ValidateNummyHttpLoggerOptions(NummyHttpLoggerOptions options)
    {
        if (string.IsNullOrEmpty(options.DatabaseConnectionString?.Trim()))
            throw new NummyHttpLoggerOptionsValidationException();
    }
}