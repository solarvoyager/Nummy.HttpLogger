using Nummy.HttpLogger.Utils.Exceptions;

namespace Nummy.HttpLogger.Utils;

internal static class NummyValidators
{
    public static void ValidateNummyHttpLoggerOptions(NummyHttpLoggerOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.DsnUrl))
            throw new NummyHttpLoggerOptionsValidationException();
    }
}