using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Goober.Logging
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate next;

        public LoggingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            string headers = string.Empty;
            foreach (var key in context.Request.Headers.Keys)
                headers += key + "=" + context.Request.Headers[key] + Environment.NewLine;

            context.Items["RESPONSE_CODE"] = context.Response?.StatusCode.ToString();
            context.Items["USER_IDENTITY"] = context.User?.Identity?.Name;
            context.Items["REQUEST_HEADERS"] = headers;
            context.Items["SESSION_ID"] = context.Request?.Headers["MS-ASPNETCORE-TOKEN"].ToString();

            await next(context);
        }

        private string GetDocumentContents(Stream request)
        {
            string documentContents;
            using (Stream receiveStream = request)
            {

                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    documentContents = readStream.ReadToEnd();
                }
            }
            return documentContents;
        }
    }
}
