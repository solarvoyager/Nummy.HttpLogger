namespace Nummy.HttpLogger.Data.Entitites;

internal class NummyRequestLog
{
    public int NummyRequestLogId { get; set; }
    public required string HttpLogGuid { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public required string Body { get; set; }
    public required string Path { get; set; }
    public required string RemoteIpAddress { get; set; }
}