namespace Nummy.HttpLogger.Utils;

internal class NummyConstants
{
    public const string ClientName = "NummyHttpLoggerClient";
    public const string RequestLogAddUrl = "/api/log/add/request";
    public const string ResponseLogAddUrl = "/api/log/add/response";

    public const string MaskedValue = "[MASKED]";
    public const string BinaryContentValue = "[BINARY CONTENT]";
    public const string TruncatedSuffix = "...(truncated)";
}