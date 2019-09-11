using System.Net.Http;
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
}
