using System;
using Routing.Parsing;
using Routing.SegmentRegistryFacadeImplementation;

namespace Routing
{
    public class RouteRegistryBuilder<TRequest, TResponse>
    {
        private readonly IRouter<TRequest, TResponse> _router;

        private RouteRegistryBuilder(IRouter<TRequest, TResponse> router)
        {
            _router = router;
        }

        public static RouteRegistryBuilder<TRequest, TResponse> WithFallbackRequestHandler(
            Func<TRequest, TResponse> handleFallbackRequest) =>
            WithCustomRouter(new Router<TRequest, TResponse>(
                    new PathParser(),
                    handleFallbackRequest));

        public static RouteRegistryBuilder<TRequest, TResponse> WithCustomRouter(
            IRouter<TRequest, TResponse> router) =>
            new RouteRegistryBuilder<TRequest, TResponse>(router);

        public IRouteRegistry<TRequest, TResponse> Build()
        {
            var segmentParser = new SegmentParser();

            return new RouteRegistryFacade<TRequest, TResponse>(
                new RouteRemover<TRequest, TResponse>(segmentParser),
                new RouteRegistrar<TRequest, TResponse>(segmentParser),
                _router);
        }
    }
}
