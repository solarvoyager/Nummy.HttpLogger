using System.Net;

namespace Nummy.HttpLogger.Models
{
    public class NummyHttpLoggerOptions
    {
        public bool ReturnResponseDuringException { get; set; }
        public object Response { get; set; }
        public HttpStatusCode ResponseStatusCode { get; set; } = HttpStatusCode.BadRequest;
    }
}
