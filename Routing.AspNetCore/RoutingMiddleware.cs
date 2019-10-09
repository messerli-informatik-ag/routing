using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Messerli.Routing.AspNetCore
{
    public delegate Task ApplyResponseToContext<in TResponse>(HttpContext context, TResponse response);

    public delegate TRequest MapContextToRequest<out TRequest>(HttpContext context);

    internal class RoutingMiddleware<TRequest, TResponse>
    {
        private readonly IRouteRegistry<TRequest, TResponse> _routeRegistry;

        private readonly MapContextToRequest<TRequest> _mapContextToRequest;

        private readonly ApplyResponseToContext<TResponse> _applyResponseToContext;

        public RoutingMiddleware(
            RequestDelegate next,
            IRouteRegistry<TRequest, TResponse> routeRegistry,
            MapContextToRequest<TRequest> mapContextToRequest,
            ApplyResponseToContext<TResponse> applyResponseToContext)
        {
            _routeRegistry = routeRegistry;
            _mapContextToRequest = mapContextToRequest;
            _applyResponseToContext = applyResponseToContext;
        }

        public async Task Invoke(HttpContext context)
        {
            var method = new HttpMethod(context.Request.Method);
            var endpoint = new Endpoint(method, context.Request.Path);
            var request = _mapContextToRequest(context);

            var response = _routeRegistry.Route(endpoint, request);
            await _applyResponseToContext(context, response);
        }
    }
}
