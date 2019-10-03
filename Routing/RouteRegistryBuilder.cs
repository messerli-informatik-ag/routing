using System;

namespace Routing
{
    public class RouteRegistryBuilder<TRequest, TResponse>
    {
        private readonly Func<TRequest, TResponse> _handleFallbackRequest;

        private RouteRegistryBuilder(Func<TRequest, TResponse> handleFallbackRequest)
        {
            _handleFallbackRequest = handleFallbackRequest;
        }

        public static RouteRegistryBuilder<TRequest, TResponse> WithFallbackRequestHandler(
            Func<TRequest, TResponse> handleFallbackRequest)
        {
            return new RouteRegistryBuilder<TRequest, TResponse>(handleFallbackRequest);
        }

        public IRouteRegistry<TRequest, TResponse> Build() =>
            new RouteRegistryFacade<TRequest, TResponse>(_handleFallbackRequest);
    }
}
