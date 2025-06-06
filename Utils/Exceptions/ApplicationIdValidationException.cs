namespace Nummy.HttpLogger.Utils.Exceptions;

internal class ApplicationIdValidationException()
    : NummyHttpLoggerException($"{nameof(NummyHttpLoggerOptions.ApplicationId)} must have a valid Guid value. Make sure to it copied from the Nummy.");