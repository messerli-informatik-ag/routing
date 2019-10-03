using System.Net.Http;

namespace Routing.SegmentRegistryFacadeImplementation
{
    internal interface IRouteRemover<TRequest, TResponse>
    {
        void Remove(SegmentNode<TRequest, TResponse> segmentTree, Endpoint endpoint);
    }
}
