namespace Nummy.HttpLogger.Data.Entitites;

internal class NummyResponseLog
{
    public int NummyResponseLogId { get; set; }
    public required string HttpLogGuid { get; set; }
    public required string ResponseBody { get; set; }
}