using System.Net.Http;
using RouteParams = System.Collections.Generic.IDictionary<string, string>;

namespace Routing
{
    public delegate TResponse HandleRequest<out TResponse, in TRequest>(TRequest request, RouteParams routeParams);

    public interface IRouteRegistry<TRequest, TResponse>
    {
        TResponse Route(HttpMethod method, string path, TRequest request);

        IRouteRegistry<TRequest, TResponse> Register(HttpMethod method, string route, HandleRequest<TRequest, TResponse> handleRequest);

        IRouteRegistry<TRequest, TResponse> Remove(HttpMethod method, string route);
    }
}
