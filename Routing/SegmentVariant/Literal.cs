#pragma warning disable 660,661

namespace Routing.SegmentVariant
{
    [Equals]
    internal class Literal : ISegmentVariant
    {
        internal Literal(string identifier)
        {
            Identifier = identifier;
        }

        internal string Identifier { get; }

        public static bool operator==(Literal left, Literal right) => Operator.Weave(left, right);

        public static bool operator!=(Literal left, Literal right) => Operator.Weave(left, right);
    }
}
