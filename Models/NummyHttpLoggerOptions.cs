﻿namespace Nummy.HttpLogger.Models;

public class NummyHttpLoggerOptions
{
    public bool EnableRequestLogging { get; set; } = true;
    public bool EnableResponseLogging { get; set; } = true;
    public string[] ExcludeContainingPaths { get; set; } = Array.Empty<string>();
    public NummyHttpLoggerDatabaseType DatabaseType { get; set; }
    public string DatabaseConnectionString { get; set; }
}