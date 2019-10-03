namespace Routing.SegmentRegistryFacadeImplementation
{
    public interface IRouteRemover<TRequest, TResponse>
    {
        void Remove(SegmentNode<TRequest, TResponse> segmentTree, Endpoint endpoint);
    }
}
