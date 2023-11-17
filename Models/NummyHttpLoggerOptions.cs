namespace Nummy.HttpLogger.Models
{
    public class NummyHttpLoggerOptions
    {
        public bool EnableRequestLoggging { get; set; } = true;
        public bool EnableResponseLoggging { get; set; } = true;
        public string[] ExcludeContainingPaths { get; set; } = new string[0];
        public NummyHttpLoggerDatabaseType DatabaseType { get; set; }
        public string DatabaseConnectionString { get; set; }
    }
}
