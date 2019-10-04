using System;
using Messerli.Routing.Parsing;
using Messerli.Routing.SegmentRegistryFacadeImplementation;

namespace Messerli.Routing
{
    public class RouteRegistryBuilder<TRequest, TResponse>
    {
        private readonly IRouter<TRequest, TResponse> _router;

        private readonly IRouteRemover<TRequest, TResponse>? _routeRemover;

        private readonly IRouteRegistrar<TRequest, TResponse>? _routeRegistrar;

        private readonly ISegmentParser? _segmentParser;

        private RouteRegistryBuilder(IRouter<TRequest, TResponse> router)
        {
            _router = router;
        }

        private RouteRegistryBuilder(
            IRouter<TRequest, TResponse> router,
            IRouteRemover<TRequest, TResponse>? routeRemover,
            IRouteRegistrar<TRequest, TResponse>? routeRegistrar,
            ISegmentParser? segmentParser)
        {
            _router = router;
            _routeRemover = routeRemover;
            _routeRegistrar = routeRegistrar;
            _segmentParser = segmentParser;
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

        public RouteRegistryBuilder<TRequest, TResponse> SetRouteRemover(IRouteRemover<TRequest, TResponse> routeRemover) =>
            ShallowClone(routeRemover: routeRemover);

        public RouteRegistryBuilder<TRequest, TResponse> SetRouteRegistrar(IRouteRegistrar<TRequest, TResponse> routeRegistrar) =>
            ShallowClone(routeRegistrar: routeRegistrar);

        public RouteRegistryBuilder<TRequest, TResponse> SetSegmentParser(ISegmentParser segmentParser) =>
            ShallowClone(segmentParser: segmentParser);

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

        private RouteRegistryBuilder<TRequest, TResponse> ShallowClone(
            IRouteRemover<TRequest, TResponse>? routeRemover = null,
            IRouteRegistrar<TRequest, TResponse>? routeRegistrar = null,
            ISegmentParser? segmentParser = null)
        {
            return new RouteRegistryBuilder<TRequest, TResponse>(
                _router,
                routeRemover ?? _routeRemover,
                routeRegistrar ?? _routeRegistrar,
                segmentParser ?? _segmentParser);
        }
    }
}
