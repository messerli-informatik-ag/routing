using System;
using Routing.Parsing;
using Routing.SegmentRegistryFacadeImplementation;

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

        public IRouteRegistry<TRequest, TResponse> Build()
        {
            var segmentParser = new SegmentParser();
            var pathParser = new PathParser();

            return new RouteRegistryFacade<TRequest, TResponse>(
                new RouteRemover<TRequest, TResponse>(segmentParser),
                new RouteRegistrar<TRequest, TResponse>(segmentParser),
                new Router<TRequest, TResponse>(pathParser, _handleFallbackRequest));
        }
    }
}
