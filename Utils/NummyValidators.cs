using Nummy.HttpLogger.Utils.Exceptions;

namespace Nummy.HttpLogger.Utils;

internal static class NummyValidators
{
    public static void ValidateNummyHttpLoggerOptions(NummyHttpLoggerOptions options)
    {
        var isValidApplicationId = !string.IsNullOrWhiteSpace(options.ApplicationId) &&
                                   Guid.TryParse(options.ApplicationId, out var guid) &&
                                   guid != Guid.Empty;
        
        var isValidNummyServiceUrl = !string.IsNullOrWhiteSpace(options.NummyServiceUrl) &&
                                     Uri.TryCreate(options.NummyServiceUrl, UriKind.Absolute, out var uri);

        if (!isValidApplicationId)
            throw new ApplicationIdValidationException();

        if(!isValidNummyServiceUrl)
            throw new NummyServiceUrlValidationException();

        if (options.MaxBodyLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(options.MaxBodyLength),
                "MaxBodyLength must be greater than zero.");

        if (options.HttpClientTimeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(options.HttpClientTimeout),
                "HttpClientTimeout must be greater than zero.");
    }
}