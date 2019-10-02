using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Routing.AspNetCore
{
    internal class RoutingMiddleware<TRequest, TResponse>
    {
        private readonly RequestDelegate _next;

        private readonly ILogger _logger;

        private readonly IRouteRegistry<TRequest, TResponse> _routeRegistry;
        
        public RoutingMiddleware(
            RequestDelegate next,
            ILogger logger,
            IRouteRegistry<TRequest, TResponse> routeRegistry)
        {
            _next = next;
            _logger = logger;
            _routeRegistry = routeRegistry;
        }

        public async Task Invoke(HttpContext context)
        {
            var request = new Request(context);
            var response = new Response(context);

            await lifecycleHandler.OnRequest(request);

            RegisterOnResponseStartingHandler(context, async () =>
            {
                _routeRegistry.Route(context.Request.Method, context.Request.Path, response);
            });

            context.Features.Set(lifecycleHandler.Session);

            try
            {
                await _next(context);
            }
            finally
            {
                // Todo: Idk what to put here lol
                context.Features[typeof(ISession)] = null;
            }
        }

        private void RegisterOnResponseStartingHandler(
            HttpContext context,
            Func<Task> onResponseAction)
        {
            context.Response.OnStarting(async () =>
            {
                try
                {
                    await onResponseAction();
                }
                catch (Exception exception)
                {
                    _logger.ErrorSavingTheSession(exception);
                }
            });
        }
    }
}
