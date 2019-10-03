using System.Net.Http;

namespace Routing.SegmentRegistryFacadeImplementation
{
    internal interface IRouteRegistrar<TRequest, TResponse>
    {
        void Register(
            SegmentNode<TRequest, TResponse> segmentTree,
            HttpMethod method,
            string route,
            HandleRequest<TRequest, TResponse> handleRequest);
    }
}
