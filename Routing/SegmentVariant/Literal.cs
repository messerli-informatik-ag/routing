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
    }
}
