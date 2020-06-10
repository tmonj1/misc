using System;
using System.Reflection;
using System.Linq;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Formatting;
using Serilog.Core;

namespace SerilogDemo
{
    /// <summary>
    /// Null sink
    /// </summary>
    public class NullSink : ILogEventSink
    {
        /// <summary>
        /// Do nothing.
        /// </summary>
        /// <param name="logEvent"></param>
        public void Emit(LogEvent logEvent) { }
    }

    /// <summary>
    /// Extension methods for Serilog
    /// </summary>
    public static class SerilogLoggerConfigurationExtension
    {
        /// <summary>
        /// Sets minimum level for the logger.
        /// </summary>
        /// <param name="loggerMinimum"></param>
        /// <param name="minLevel">ログ出力レベル。
        /// <para>nullまたは不正な文字列が指定された場合、デフォルトで"Information"レベルに設定されます。</param>
        /// <returns>LoggerConfiguration</returns>
        public static LoggerConfiguration DefaultTo(this LoggerMinimumLevelConfiguration loggerMinimum, string minLevel)
        {
            LogEventLevel level;
            if (!string.IsNullOrEmpty(minLevel) && Enum.TryParse(minLevel, true, out level))
            {
                return loggerMinimum.Is(level);
            }
            else
            {
                return loggerMinimum.Information();
            }
        }

        /// <summary>
        /// "Microsoft"と"System"のログ出力レベルを設定する。
        /// </summary>
        /// <param name="loggerMinimum"></param>
        /// <param name="source">設定対象 ("Microsoft"または"System"</param>
        /// <param name="minLevel">ログ出力レベル
        /// <para>nullまたは解釈できない不正な文字列が指定された場合、デフォルトで"Warning"レベルに設定します。
        /// </param>
        /// <returns>LoggerConfiguration</returns>
        public static LoggerConfiguration OverrideWith(this LoggerMinimumLevelConfiguration loggerMinimum, string source, string minLevel)
        {
            LogEventLevel level;
            if (string.IsNullOrEmpty(minLevel) || !Enum.TryParse(minLevel, true, out level))
            {
                level = LogEventLevel.Warning;
            }

            return loggerMinimum.Override(source, level);
        }

        /// <summary>
        /// コンソール出力時のフォーマッタを設定する。 
        /// </summary>
        /// <param name="loggerSinkConfiguration"></param>
        /// <param name="formatterName?">フォーマッタのクラス名(完全修飾名),アセンブリ名
        /// (例: Serilog.Formatting.Json.JsonFormatter, Serilog)
        /// <para>nullまたはFormatterとして解釈できない不正な文字列を指定した場合、代わりにデフォルトフォーマッタ
        /// (JsonFormatter) を使います。</para>
        /// <example>
        ///   <list type="bullet">
        ///     <item>JsonFormatter</term>
        ///     <description>Serilog.Formatting.Json.JsonFormatter,Serilog</description>
        ///     <item>CompactJsonFormatter</item>
        ///     <description>Serilog.Formatting.Compact.CompactJsonFormatter,Serilog.Formatting.Compact</description>
        ///     <item>ElasticsearchJsonFormatter</item>
        ///     <description>Serilog.Formatting.Elasticsearch.ElasticsearchJsonFormatter,Serilog.Formatting.Elasticsearch</description>
        ///   </list>
        /// </example>
        /// </param>
        /// <returns>LoggerConfiguration</returns>
        public static LoggerConfiguration ConsoleWithFormatter(this LoggerSinkConfiguration loggerSinkConfiguration, string formatterName)
        {
            try
            {
                // formatterName := <ClassName>,<AssemblyName>
                string[] values = formatterName?.Replace(" ", "").Split(",");
                if (values.Length == 2)
                {
                    (string className, string assemblyName) = (values[0], values[1]);

                    // load the assembly
                    var assembly = Assembly.Load(assemblyName);

                    // load the formatter class
                    Type type = assembly?.GetType(className);

                    // create an Instance with all default params
                    ParameterInfo[] paramInfos = type?.GetConstructors()?.FirstOrDefault<ConstructorInfo>()?.GetParameters();
                    object[] ctorParams = paramInfos?.Select(p => p.DefaultValue)?.ToArray();
                    ITextFormatter formatter = Activator.CreateInstance(type, ctorParams) as ITextFormatter;

                    // configure console with the specified formatter 
                    if (formatter != null)
                    {
                        return loggerSinkConfiguration.Console(formatter);
                    }
                }
            }
            catch (Exception e)
            {
                // go thru and use the default formatter if something went wrong
                Console.WriteLine($"wrong log formatter: {formatterName}. The default formatter is used instead. {e.ToString()}");
            }

            // use default Formatter (JsonFormatter)
            return loggerSinkConfiguration.Console();
        }

        /// <summary>
        /// Sets SEQ sink only if url is provided.
        /// </summary>
        /// <param name="loggerSinkConfiguration">LoggerSinkConfiguration</param>
        /// <param name="url">url for seq.</param>
        /// <returns>LoggerConfiguration</returns>
        public static LoggerConfiguration SeqWithUrl(this LoggerSinkConfiguration loggerSinkConfiguration, string url)
        {
            // use SEQ if url is specified
            return string.IsNullOrEmpty(url)
                ? loggerSinkConfiguration.Sink(new NullSink(), LogEventLevel.Fatal)
                : loggerSinkConfiguration.Seq(url);
        }
    }
}