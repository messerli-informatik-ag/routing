using Microsoft.AspNetCore.Builder;

namespace Messerli.Routing.AspNetCore
{
    public static class RoutingMiddlewareExtension
    {
        public static IApplicationBuilder UseRouting<TRequest, TResponse>(
            this IApplicationBuilder applicationBuilder,
            IRouteRegistry<TRequest, TResponse> routeRegistry,
            MapContextToRequest<TRequest> mapContextToRequest,
            ApplyResponseToContext<TResponse> applyResponseToContext)
        {
            return applicationBuilder.UseMiddleware<RoutingMiddleware<TRequest, TResponse>>(
                routeRegistry,
                mapContextToRequest,
                applyResponseToContext);
        }
    }
}
