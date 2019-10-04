using System;
using Messerli.Routing.Parsing;
using Messerli.Routing.SegmentRegistryFacadeImplementation;

namespace Messerli.Routing
{
    public class RouteRegistryBuilder<TRequest, TResponse>
    {
        private readonly IRouter<TRequest, TResponse> _router;

        private readonly ISegmentParser? _segmentParser;

        private RouteRegistryBuilder(IRouter<TRequest, TResponse> router)
        {
            _router = router;
        }

        private RouteRegistryBuilder(
            IRouter<TRequest, TResponse> router,
            ISegmentParser? segmentParser)
        {
            _router = router;
            _segmentParser = segmentParser;
        }

        public static RouteRegistryBuilder<TRequest, TResponse> WithFallbackRequestHandler(
            Func<TRequest, TResponse> handleFallbackRequest) =>
            WithCustomPathParserAndFallbackRequestHandler(new PathParser(), handleFallbackRequest);

        public static RouteRegistryBuilder<TRequest, TResponse> WithCustomPathParserAndFallbackRequestHandler(
           IPathParser pathParser,
           Func<TRequest, TResponse> handleFallbackRequest) =>
            new RouteRegistryBuilder<TRequest, TResponse>(new Router<TRequest, TResponse>(
                pathParser,
                handleFallbackRequest));

        public RouteRegistryBuilder<TRequest, TResponse> SegmentParser(ISegmentParser segmentParser) =>
            ShallowClone(segmentParser: segmentParser);

        public IRouteRegistry<TRequest, TResponse> Build()
        {
            var segmentParser = _segmentParser ?? new SegmentParser();

            var routeRemover = new RouteRemover<TRequest, TResponse>(segmentParser);
            var routeRegistrar = new RouteRegistrar<TRequest, TResponse>(segmentParser);

            return new RouteRegistryFacade<TRequest, TResponse>(
                routeRemover,
                routeRegistrar,
                _router);
        }

        private RouteRegistryBuilder<TRequest, TResponse> ShallowClone(
            ISegmentParser? segmentParser = null)
        {
            return new RouteRegistryBuilder<TRequest, TResponse>(
                _router,
                segmentParser ?? _segmentParser);
        }
    }
}
