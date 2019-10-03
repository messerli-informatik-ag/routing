﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

namespace Routing.AspNetCore
{
    public static class RoutingMiddlewareExtension
    {
        public static IApplicationBuilder UseRouting<TRequest, TResponse>(
            this IApplicationBuilder applicationBuilder,
            MapContextToRequest<TRequest> mapContextToRequest,
            ApplyResponseToContext<TResponse> applyResponseToContext,
            Func<TRequest, TResponse> handleFallbackRequest)
        {
            var loggerFactory = ResolveApplicationService<ILoggerFactory>(applicationBuilder);
            var logger = loggerFactory.CreateLogger<RoutingMiddleware<TRequest, TResponse>>();
            var routeRegistry = new RouteRegistryFacade<TRequest, TResponse>(handleFallbackRequest);
            return applicationBuilder.UseMiddleware<RoutingMiddleware<TRequest, TResponse>>(
                logger,
                routeRegistry,
                mapContextToRequest,
                applyResponseToContext);
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
