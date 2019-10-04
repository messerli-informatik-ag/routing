namespace Messerli.Routing.SegmentRegistryFacadeImplementation
{
    public interface IRouter<TRequest, TResponse>
    {
        TResponse Route(SegmentNode<TRequest, TResponse> segmentTree, Endpoint endpoint, TRequest request);
    }
}
