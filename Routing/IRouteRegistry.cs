using System.Net.Http;
using RouteParams = System.Collections.Generic.IDictionary<string, string>;

namespace Messerli.Routing
{
    public delegate TResponse HandleRequest<in TRequest, out TResponse>(TRequest request, RouteParams routeParams);

    public interface IRouteRegistry<TRequest, TResponse>
    {
        TResponse Route(HttpMethod method, string path, TRequest request);

        IRouteRegistry<TRequest, TResponse> Register(HttpMethod method, string route, HandleRequest<TRequest, TResponse> handleRequest);

        IRouteRegistry<TRequest, TResponse> Remove(HttpMethod method, string route);
    }
}
