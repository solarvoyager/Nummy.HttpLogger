namespace Nummy.HttpLogger.Data.Entitites;

internal class NummyRequestLog
{
    public required Guid HttpLogId { get; set; }
    public required string TraceIdentifier { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public required bool IsDeleted { get; set; }
    public required string Body { get; set; }
    public required string Method { get; set; }
    public required string Path { get; set; }
    public string? RemoteIp { get; set; }
}