using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Goober.Logging
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = loggerFactory?.CreateLogger<ErrorHandlingMiddleware>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }


        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var originalExc = GetInnerException(ex);
                var message = $"Message: {originalExc.Message}, StackTrace: {originalExc.StackTrace}";
                _logger.LogError(message: message);
                throw;
            }
        }

        private Exception GetInnerException(Exception exc)
        {
            var maxCount = 50;
            var iterator = 0;
            while (exc.InnerException != null)
            {
                if (iterator >= maxCount)
                    return exc;

                iterator++;
                exc = exc.InnerException;
            }

            return exc;
        }
    }
}
