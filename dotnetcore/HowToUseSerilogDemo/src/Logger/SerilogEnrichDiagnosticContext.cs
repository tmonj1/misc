using System;
using Serilog;
using Microsoft.AspNetCore.Http;

namespace SerilogDemo.Logger
{
    /// <summary>
    /// Serilogリクエストロギングにカスタムヘッダのロギング機能を追加します
    /// </summary>
    public class RequestLoggingEnricher
    {
        /// <summary>
        /// リクエストロギングの出力項目として追加するカスタムヘッダの配列
        /// </summary>
        /// <value>string[]</value>
        public string[] _CustomHeaders { get; set; } = new string[0];

        /// <summary>
        /// リクエストロギングの出力項目としてHttp Protocol Versionを追加するかどうか
        /// </summary>
        /// <value>bool</value>
        public bool _HttpVersionAdded { get; set; } = false;

        /// <summary>
        /// リクエストロギングの出力項目として追加するカスタムヘッダの配列
        /// </summary>
        /// <param name="headerNames"></param>
        /// <returns>RequestLoggingEnricher</returns>
        public RequestLoggingEnricher AddCustomHeaders(params string[] headerNames)
        {
            _CustomHeaders = headerNames;
            return this;
        }

        /// <summary>
        /// リクエストロギングの出力項目としてHTTPプロトコルのバージョンを追加するかどうか
        /// </summary>
        /// <param name="yes"></param>
        /// <returns>RequestLoggingEnricher</returns>
        public RequestLoggingEnricher AddHttpVersion()
        {
            _HttpVersionAdded = true;
            return this;
        }

        /// <summary>
        /// リクエストコンテキストを指定の条件でログを出力するメソッドを構築します
        /// </summary>
        /// <param name="headerNames">追加するHTTPヘッダ名の可変長配列</param>
        /// <returns>Action&lt;IDiagnosticContext, HttpContext></returns>
        public Action<IDiagnosticContext, HttpContext> Build(Action<IDiagnosticContext, HttpContext> addMore = null)
        {
            // すべてのヘッダをリクエストロギングコンテキストに追加
            return (IDiagnosticContext diagnosticContext, HttpContext httpContext) =>
            {
                // HTTP protocol version
                if (_HttpVersionAdded)
                {
                    diagnosticContext.Set("HttpVersion", httpContext.Request.Protocol);
                }

                // Custom Headers
                foreach (var headerName in (_CustomHeaders ?? new string[0]))
                {
                    string value = httpContext.Request.Headers[headerName];
                    diagnosticContext.Set(headerName, value ?? "");
                }

                // Extra Settings
                if (addMore != null)
                {
                    addMore(diagnosticContext, httpContext);
                }
            };
        }
    }
}