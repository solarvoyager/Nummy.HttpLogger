namespace Nummy.HttpLogger.Utils.Exceptions;

internal class NummyHttpLoggerOptionsValidationException()
    : Exception($"{nameof(NummyHttpLoggerOptions.DsnUrl)} must have a valid DSN url");