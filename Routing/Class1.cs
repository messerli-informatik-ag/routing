using System.Collections.Generic;

using RouteParams = System.Collections.Generic.IDictionary<string, string>;

namespace Routing
{
    public delegate TResponse HandleRequest<out TResponse, in TRequest>(TRequest request, RouteParams routeParams);

    public interface IRouteRegistry<TResponse, TRequest>
    {
        TResponse Route(HttpMethod method, string path, TRequest request);

        void Register(HttpMethod method, string route, HandleRequest<TResponse, TRequest> handleRequest);

        void Remove(HttpMethod method, string route, HandleRequest<TResponse, TRequest> handleRequest);
    }

    public class RouteRegistry<TResponse, TRequest> : IRouteRegistry<TResponse, TRequest>
    {
        private readonly IDictionary<string, HandleRequest<TResponse, TRequest>> _registeredRoutes 
            = new Dictionary<string, HandleRequest<TResponse, TRequest>>();

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

    public enum HttpMethod
    {

    }
}
