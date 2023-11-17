using Nummy.HttpLogger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nummy.HttpLogger.Utils
{
    public static class NummyModelValidator
    {
        public static void ValidateNummyHttpLoggerOptions(NummyHttpLoggerOptions options)
        {

            if (string.IsNullOrEmpty(options.DatabaseConnectionString?.Trim()))
                throw new NummyHttpLoggerOptionsValidationException();
        }
    }
}
