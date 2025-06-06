namespace Nummy.HttpLogger.Utils;

public class NummyHttpLoggerOptions
{
    public bool EnableRequestLogging { get; set; } = true;
    public bool EnableResponseLogging { get; set; } = true;
    public string[] ExcludeContainingPaths { get; set; } = [];
    public string ApplicationId { get; set; }
    public string NummyServiceUrl { get; set; }
}