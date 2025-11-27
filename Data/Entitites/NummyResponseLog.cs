namespace Nummy.HttpLogger.Data.Entitites;

internal class NummyResponseLog
{
    public required Guid HttpLogId { get; set; }
    public string? Body { get; set; }
    public required int StatusCode { get; set; }
    public required long DurationMs { get; set; }
    public string? Headers { get; set; }
}