using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elan2mqtt
{
    public class ApplicationLogging
    {
        private static ILoggerFactory _Factory = null;

        public static ILoggerFactory GetLoggerFactory
        {
            get
            {
                if (_Factory == null)
                {
                    _Factory = ConfigureLogger();
                }
                return _Factory;
            }
            set { _Factory = value; }
        }

        public static ILoggerFactory ConfigureLogger(LogLevel minimumLevel = LogLevel.Trace)
        {

            return LoggerFactory.Create(builder =>
                builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
                })
                .SetMinimumLevel(minimumLevel)
                );
        }

        public static ILogger CreateLogger<T>() => GetLoggerFactory.CreateLogger<T>();
        public static ILogger CreateLogger(string categoryName) => GetLoggerFactory.CreateLogger(categoryName);
        public static ILogger CreateLogger(Type type) => GetLoggerFactory.CreateLogger(type);

    }
}
