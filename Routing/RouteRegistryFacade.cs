using System;
using System.Net.Http;
using Routing.Parsing;
using Routing.SegmentRegistryFacadeImplementation;
using Routing.SegmentVariant;

namespace Routing
{
    public class RouteRegistryFacade<TRequest, TResponse> : IRouteRegistry<TRequest, TResponse>
    {
        private readonly SegmentNode<TRequest, TResponse> _segmentTree
            = new SegmentNode<TRequest, TResponse>(new Root());
        
        private readonly IRouteRemover<TRequest, TResponse> _routeRemover;

        private readonly IRouteRegistrar<TRequest, TResponse> _routeRegistrar;

        private readonly IRouter<TRequest, TResponse> _router;

        public RouteRegistryFacade(Func<TRequest, TResponse> handleFallbackRequest)
        {
            var segmentParser = new SegmentParser();
            _routeRemover = new RouteRemover<TRequest, TResponse>(segmentParser);
            _routeRegistrar = new RouteRegistrar<TRequest, TResponse>(segmentParser);
            var pathParser = new PathParser();
            _router = new Router<TRequest, TResponse>(pathParser, handleFallbackRequest);
        }

        public TResponse Route(HttpMethod method, string path, TRequest request)
            => _router.Route(_segmentTree, method, path, request);

        public IRouteRegistry<TRequest, TResponse> Register(HttpMethod method, string route, HandleRequest<TRequest, TResponse> handleRequest)
        {
            _routeRegistrar.Register(_segmentTree, method, route, handleRequest);
            return this;
        }
        
        public IRouteRegistry<TRequest, TResponse> Remove(HttpMethod method, string route)
        {
            _routeRemover.Remove(_segmentTree, method, route);
            return this;
        }
    }
}
