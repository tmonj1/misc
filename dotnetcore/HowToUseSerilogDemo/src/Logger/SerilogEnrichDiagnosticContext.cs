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
        /// 指定したHTTPヘッダをリクエストロギングに追加します
        /// </summary>
        /// <param name="headerNames">追加するHTTPヘッダ名の可変長配列</param>
        /// <returns>Action&lt;IDiagnosticContext, HttpContext></returns>
        public static Action<IDiagnosticContext, HttpContext> EnrichWithCustomHeaders(params string[] headerNames)
        {
            // 追加ヘッダがないとき
            if (headerNames == null)
            {
                return null;
            }

            // すべてのヘッダをリクエストロギングコンテキストに追加
            return (IDiagnosticContext diagnosticContext, HttpContext httpContext) =>
            {
                foreach (var headerName in headerNames)
                {
                    string value = httpContext.Request.Headers[headerName];
                    diagnosticContext.Set(headerName, value ?? "");
                }
            };
        }
    }
}