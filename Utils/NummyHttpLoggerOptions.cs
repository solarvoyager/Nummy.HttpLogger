namespace Nummy.HttpLogger.Utils;

public class NummyHttpLoggerOptions
{
    public bool EnableRequestLogging { get; set; } = true;
    public bool EnableResponseLogging { get; set; } = true;
    /// <summary>
    /// Paths whose URL contains any of these substrings (case-insensitive) are excluded from logging.
    /// This is a substring check, not an exact match. Example: "swagger" excludes "/swagger/index.html".
    /// </summary>
    public HashSet<string> ExcludeContainingPaths { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> MaskHeaders { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public string ApplicationId { get; set; } = null!;
    public string NummyServiceUrl { get; set; } = null!;
    public int MaxBodyLength { get; set; } = 32768; // 32 KB
    public TimeSpan HttpClientTimeout { get; set; } = TimeSpan.FromSeconds(5);
}
