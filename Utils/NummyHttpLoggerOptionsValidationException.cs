using Nummy.HttpLogger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nummy.HttpLogger.Utils
{
    public class NummyHttpLoggerOptionsValidationException : Exception
    {
        public NummyHttpLoggerOptionsValidationException()
            : base($"{nameof(NummyHttpLoggerOptions.DatabaseConnectionString)} must have a valid connection string") { }
    }
}
