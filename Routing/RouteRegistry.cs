using System.Collections.Generic;
using System.Net.Http;

namespace Routing
{
    public class RouteRegistry<TResponse, TRequest> : IRouteRegistry<TResponse, TRequest>
    {
        private readonly IDictionary<string, HandleRequest<TResponse, TRequest>> _registeredRoutes 
            = new Dictionary<string, HandleRequest<TResponse, TRequest>>();

        private readonly HandleRequest<TResponse, TRequest> _fallbackHandleRequest;

        public RouteRegistry(HandleRequest<TResponse, TRequest> fallbackHandleRequest)
        {
            _fallbackHandleRequest = fallbackHandleRequest;
        }

        public TResponse Route(HttpMethod method, string path, TRequest request)
        {
            var segments = path.Split('/');

            var routeParams = new Dictionary<string, string> { { "name", "Rubby" }, { "age", "69" } };

            return _registeredRoutes[path](request, routeParams);
        }
        public void Register(HttpMethod method, string route, HandleRequest<TResponse, TRequest> handleRequest)
        {
            _registeredRoutes[route] = handleRequest;
        }

        public void Remove(HttpMethod method, string route, HandleRequest<TResponse, TRequest> handleRequest)
        {

        }
    }
}
