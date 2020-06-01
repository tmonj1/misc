using System;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Formatting;

namespace SerilogDemo
{
    public static class SerilogLoggerConfigurationExtension
    {
        public static LoggerConfiguration ConfiguredTo(this LoggerMinimumLevelConfiguration loggerMinimum, string minLevel)
        {
            LoggerConfiguration loggerConfiguration = null;
            LogEventLevel level;
            if (Enum.TryParse(minLevel, true, out level))
            {
                loggerConfiguration = loggerMinimum.Is(level);
            }
            else
            {
                loggerConfiguration = loggerMinimum.Information();
            }

            return loggerConfiguration;
        }

        public static LoggerConfiguration ConfiguredTo(this LoggerMinimumLevelConfiguration loggerMinimum, string source, string minLevel)
        {
            LogEventLevel level;
            if (!Enum.TryParse(minLevel, true, out level))
            {
                level = LogEventLevel.Warning;
            }

            return loggerMinimum.Override(source, level);
        }

        public static LoggerConfiguration ConsoleWithFormatter(this LoggerSinkConfiguration loggerSinkConfiguration, string formatterName)
        {
            if (!string.IsNullOrEmpty(formatterName))
            {
                try
                {
                    Type type = Type.GetType(formatterName);
                    if (type != null)
                    {
                        ITextFormatter formatter = Activator.CreateInstance(type) as ITextFormatter;
                        if (formatter != null)
                        {
                            return loggerSinkConfiguration.Console(formatter);
                        }
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"wrong log formatter: {formatterName}. use CompactJsonFormatter instead.")
                    ; // go thru and write to console with default formatter                    
                }
            }

            return loggerSinkConfiguration.Console(new CompactJsonFormatter());
        }
    }
}