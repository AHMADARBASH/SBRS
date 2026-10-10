using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SBRS.Application.Common.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    {
        public async Task<TResponse> Handle(
            TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            var name = typeof(TRequest).Name;
            var sw = Stopwatch.StartNew();

            logger.LogInformation("Handling {Request}", name);
            var response = await next();
            logger.LogInformation("Handled {Request} in {Elapsed} ms", name, sw.ElapsedMilliseconds);

            return response;
        }
    }
}
