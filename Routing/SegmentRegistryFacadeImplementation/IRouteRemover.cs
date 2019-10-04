namespace Messerli.Routing.SegmentRegistryFacadeImplementation
{
    internal interface IRouteRemover<TRequest, TResponse>
    {
        void Remove(SegmentNode<TRequest, TResponse> segmentTree, Endpoint endpoint);
    }
}
