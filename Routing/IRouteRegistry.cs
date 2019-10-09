using System;
using System.Collections.Generic;
using System.Net.Http;
using RouteParams = System.Collections.Generic.IDictionary<string, string>;

namespace Messerli.Routing
{
    public delegate TResponse HandleRequest<in TRequest, out TResponse>(TRequest request, RouteParams routeParams);

    /// <summary>
    /// Validation callback that is expected to throw an exception when the route parameters are invalid.
    /// </summary>
    /// <exception cref="Exception">Throw something when the parameters are invalid according to your own business logic.</exception>
    public delegate void ValidateParameterKeys(IEnumerable<string> parameters);

    public interface IRouteRegistry<TRequest, TResponse>
    {
        TResponse CallFallbackHandler(TRequest request);

        TResponse Route(HttpMethod method, string path, TRequest request);

        IRouteRegistry<TRequest, TResponse> Register(HttpMethod method, string route, HandleRequest<TRequest, TResponse> handleRequest);

        IRouteRegistry<TRequest, TResponse> Register(HttpMethod method, string route, HandleRequest<TRequest, TResponse> handleRequest, ValidateParameterKeys validateParameterKeys);

        IRouteRegistry<TRequest, TResponse> Remove(HttpMethod method, string route);
    }
}
