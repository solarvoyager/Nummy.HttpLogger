namespace Nummy.HttpLogger.Utils.Exceptions;

internal class NummyServiceUrlValidationException()
    : NummyHttpLoggerException($"{nameof(NummyHttpLoggerOptions.NummyServiceUrl)} must have a valid Uri value. Make sure it is copied from Nummy.");