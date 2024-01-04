namespace Nummy.HttpLogger.Data.Entitites;

internal class NummyResponseLog
{
    public required Guid HttpLogId { get; set; }
    public required string Body { get; set; }
    public required int StatusCode { get; set; }
}