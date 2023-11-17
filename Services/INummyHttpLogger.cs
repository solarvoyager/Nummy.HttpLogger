namespace Nummy.HttpLogger.Services
{
    internal interface INummyHttpLogger
    {
        string LogRequest(object request);
        void LogResponse(int requestGuid, object response);
    }
}
