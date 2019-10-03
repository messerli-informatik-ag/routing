#pragma warning disable 660,661

namespace Routing.SegmentVariant
{
    [Equals]
    internal sealed class Root : ISegmentVariant
    {
        public static bool operator ==(Root left, Root right) => Operator.Weave(left, right);

        public static bool operator !=(Root left, Root right) => Operator.Weave(left, right);
    }
}
