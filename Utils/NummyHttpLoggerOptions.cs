namespace Nummy.HttpLogger.Utils;

public class NummyHttpLoggerOptions
{
    public bool EnableRequestLogging { get; set; } = true;
    public bool EnableResponseLogging { get; set; } = true;
    public HashSet<string> ExcludeContainingPaths { get; set; } = [];
    public HashSet<string> MaskHeaders { get; set; } = [];
    public string ApplicationId { get; set; }
    public string NummyServiceUrl { get; set; }
    public int MaxBodyLength { get; set; } = 32768; // 32 KB
}