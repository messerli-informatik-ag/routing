namespace Routing
{
    [Equals]
    internal class Path : ISegmentVariant
    {
        internal Path(string identifier)
        {
            Identifier = identifier;
        }

        internal string Identifier { get; }
    }
}
