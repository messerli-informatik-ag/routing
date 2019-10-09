using System;
using System.Collections.Generic;
using System.Net.Http;
using Messerli.Routing.Parsing;
using Messerli.Routing.SegmentRegistryFacadeImplementation;
using Messerli.Routing.SegmentVariant;

namespace Messerli.Routing
{
    internal class RouteRegistryFacade<TRequest, TResponse> : IRouteRegistry<TRequest, TResponse>
    {
        private readonly SegmentNode<TRequest, TResponse> _segmentTree
            = new SegmentNode<TRequest, TResponse>(new Root());

        private readonly IRouteRemover<TRequest, TResponse> _routeRemover;

        private readonly IRouteRegistrar<TRequest, TResponse> _routeRegistrar;

        private readonly IRouter<TRequest, TResponse> _router;

        public RouteRegistryFacade(
            IRouteRemover<TRequest, TResponse> routeRemover,
            IRouteRegistrar<TRequest, TResponse> routeRegistrar,
            IRouter<TRequest, TResponse> router)
        {
            _routeRemover = routeRemover;
            _routeRegistrar = routeRegistrar;
            _router = router;
        }

        public TResponse CallFallbackHandler(TRequest request) =>
            _router.CallFallbackHandler(request);

        public TResponse Route(HttpMethod method, string path, TRequest request)
        {
            var endpoint = new Endpoint(method, path);
            return _router.Route(_segmentTree, endpoint, request);
        }

        public IRouteRegistry<TRequest, TResponse> Register(HttpMethod method, string route, HandleRequest<TRequest, TResponse> handleRequest)
        {
            var endpoint = new Endpoint(method, route);
            _routeRegistrar.Register(_segmentTree, endpoint, handleRequest);
            return this;
        }

        public IRouteRegistry<TRequest, TResponse> Register(
            HttpMethod method,
            string route,
            HandleRequest<TRequest, TResponse> handleRequest,
            ValidateParameterKeys validateParameterKeys)
        {
            var endpoint = new Endpoint(method, route);
            _routeRegistrar.Register(_segmentTree, endpoint, handleRequest, validateParameterKeys);
            return this;
        }

        public IRouteRegistry<TRequest, TResponse> Remove(HttpMethod method, string route)
        {
            var endpoint = new Endpoint(method, route);
            _routeRemover.Remove(_segmentTree, endpoint);
            return this;
        }
    }
}
