using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Routing.AspNetCore
{
    public static class RoutingMiddlewareExtension
    {
            public static IApplicationBuilder UseRouting(
                this IApplicationBuilder applicationBuilder)
            {
                var loggerFactory = ResolveApplicationService<ILoggerFactory>(applicationBuilder);
                var logger = loggerFactory.CreateLogger<RoutingMiddleware>();
                return applicationBuilder.UseMiddleware<RoutingMiddleware>(logger);
            }

  private static T ResolveApplicationService<T>(IApplicationBuilder applicationBuilder)
                where T : class
            {
                var type = typeof(T);
                return applicationBuilder.ApplicationServices.GetService(type) as T
                       ?? throw new NullReferenceException($"Unable to resolve {type.Name}");
            }
        }
    
}
