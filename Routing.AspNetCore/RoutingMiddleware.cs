using System;
using System.Net.Http;
using System.Threading.Tasks;
using Messerli.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Routing.AspNetCore
{
    public delegate Task ApplyResponseToContext<in TResponse>(HttpContext context, TResponse response);

    public delegate TRequest MapContextToRequest<out TRequest>(HttpContext context);

    internal class RoutingMiddleware<TRequest, TResponse>
    {
        private readonly ILogger _logger;

        private readonly IRouteRegistry<TRequest, TResponse> _routeRegistry;

        private readonly MapContextToRequest<TRequest> _mapContextToRequest;

        private readonly ApplyResponseToContext<TResponse> _applyResponseToContext;

        public RoutingMiddleware(
            RequestDelegate next,
            ILogger logger,
            IRouteRegistry<TRequest, TResponse> routeRegistry,
            MapContextToRequest<TRequest> mapContextToRequest,
            ApplyResponseToContext<TResponse> applyResponseToContext)
        {
            _logger = logger;
            _routeRegistry = routeRegistry;
            _mapContextToRequest = mapContextToRequest;
            _applyResponseToContext = applyResponseToContext;
        }

        public async Task Invoke(HttpContext context)
        {
            var method = new HttpMethod(context.Request.Method);
            var request = _mapContextToRequest(context);

            try
            {
                var response = _routeRegistry.Route(method, context.Request.Path, request);
                await ApplyResponseToContext(context, response);
            }
            catch (Exception exception)
            {
                _logger.ErrorWhileRouting(exception);
            }
        }

        private async Task ApplyResponseToContext(HttpContext context, TResponse response)
        {
            try
            {
                await _applyResponseToContext(context, response);
            }
            catch (Exception exception)
            {
                _logger.ErrorWhileConvertingResponseToContext(exception);
            }
        }
    }
}
