#pragma warning disable 660,661

namespace Routing.SegmentVariant
{
    [Equals]
    public sealed class Parameter : ISegmentVariant
    {
        internal Parameter(string key)
        {
            Key = key;
        }

        internal string Key { get; }

        public static bool operator ==(Parameter left, Parameter right) => Operator.Weave(left, right);

        public static bool operator !=(Parameter left, Parameter right) => Operator.Weave(left, right);
    }
}
