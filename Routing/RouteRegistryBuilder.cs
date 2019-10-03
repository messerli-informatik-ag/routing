using System;
using Routing.Parsing;
using Routing.SegmentRegistryFacadeImplementation;

namespace Routing
{
    public class RouteRegistryBuilder<TRequest, TResponse>
    {
        private readonly IRouter<TRequest, TResponse> _router;

        private IRouteRemover<TRequest, TResponse>? _routeRemover;

        private IRouteRegistrar<TRequest, TResponse>? _routeRegistrar;

        private ISegmentParser? _segmentParser;

        private RouteRegistryBuilder(IRouter<TRequest, TResponse> router)
        {
            _router = router;
        }

        public static RouteRegistryBuilder<TRequest, TResponse> WithFallbackRequestHandler(
            Func<TRequest, TResponse> handleFallbackRequest) =>
            WithCustomPathParserAndFallbackRequestHandler(new PathParser(), handleFallbackRequest);

        public static RouteRegistryBuilder<TRequest, TResponse> WithCustomPathParserAndFallbackRequestHandler(
           IPathParser pathParser,
           Func<TRequest, TResponse> handleFallbackRequest) =>
            WithCustomRouter(new Router<TRequest, TResponse>(
                pathParser,
                handleFallbackRequest));

        public static RouteRegistryBuilder<TRequest, TResponse> WithCustomRouter(
            IRouter<TRequest, TResponse> router) =>
            new RouteRegistryBuilder<TRequest, TResponse>(router);

        public RouteRegistryBuilder<TRequest, TResponse> SetRouteRemover(IRouteRemover<TRequest, TResponse> routeRemover)
        {
            _routeRemover = routeRemover;
            return this;
        }

        public RouteRegistryBuilder<TRequest, TResponse> SetRouteRegistrar(IRouteRegistrar<TRequest, TResponse> routeRegistrar)
        {
            _routeRegistrar = routeRegistrar;
            return this;
        }

        public RouteRegistryBuilder<TRequest, TResponse> SetSegmentParser(ISegmentParser segmentParser)
        {
            _segmentParser = segmentParser;
            return this;
        }

        public IRouteRegistry<TRequest, TResponse> Build()
        {
            var segmentParser = _segmentParser ?? new SegmentParser();

            var routeRemover = _routeRemover ?? new RouteRemover<TRequest, TResponse>(segmentParser);
            var routeRegistrar = _routeRegistrar ?? new RouteRegistrar<TRequest, TResponse>(segmentParser);

            return new RouteRegistryFacade<TRequest, TResponse>(
                routeRemover,
                routeRegistrar,
                _router);
        }
    }
}
