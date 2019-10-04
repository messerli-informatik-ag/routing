#pragma warning disable 660,661

namespace Messerli.Routing.SegmentVariant
{
    [Equals]
    public sealed class Literal : ISegmentVariant
    {
        internal Literal(string identifier)
        {
            Identifier = identifier;
        }

        internal string Identifier { get; }

        public static bool operator ==(Literal left, Literal right) => Operator.Weave(left, right);

        public static bool operator !=(Literal left, Literal right) => Operator.Weave(left, right);
    }
}
