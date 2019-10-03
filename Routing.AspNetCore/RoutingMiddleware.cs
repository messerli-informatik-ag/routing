using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Routing.AspNetCore
{
    public delegate Task ApplyResponseToContext<in TResponse>(HttpContext context, TResponse response);

    public delegate TRequest MapContextToRequest<out TRequest>(HttpContext context);

    internal class RoutingMiddleware<TRequest, TResponse>
    {
        private readonly ILogger _logger;

        private readonly IRouteRegistry<TResponse, TRequest> _routeRegistry;

        private readonly MapContextToRequest<TRequest> _mapContextToRequest;

        private readonly ApplyResponseToContext<TResponse> _applyResponseToContext;

        public RoutingMiddleware(
            RequestDelegate next,
            ILogger logger,
            IRouteRegistry<TResponse, TRequest> routeRegistry,
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

            var response = _routeRegistry.Route(method, context.Request.Path, request);

            await _applyResponseToContext(context, response);
        }
    }
}
