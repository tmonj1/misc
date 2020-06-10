using System;
using Serilog;
using Serilog.Core;

namespace SerilogDemo.Logger
{
    /// <summary>
    /// Serilog configuration wrapper class.
    /// </summary>
    public class SerilogConfiguration
    {
        /// <summary>
        /// Configures Serilog with Default setup.
        /// </summary>
        /// <param name="optionalSetup"></param>
        /// <returns>LoggerConfiguration</returns>
        public static LoggerConfiguration ConfigureDefault(Func<LoggerConfiguration, LoggerConfiguration> optionalSetup = null)
        {
            Func<string, string> env = (key) => Environment.GetEnvironmentVariable(key);

            // set up console logger (for fluentd)
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.DefaultTo(env("LOG_MIN_LEVEL"))
                .MinimumLevel.OverrideWith("Microsoft", env("LOG_MIN_LEVEL_MICROSOFT"))
                .MinimumLevel.OverrideWith("System", env("LOG_MIN_LEVEL_SYSTEM"))
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.FromLogContext()
                .WriteTo.ConsoleWithFormatter(env("LOG_FORMATTER"));

            // optional setup
            if (optionalSetup != null)
            {
                loggerConfiguration = optionalSetup(loggerConfiguration);
            }

            return loggerConfiguration;
        }
    }
}