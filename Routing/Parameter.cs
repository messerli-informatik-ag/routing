namespace Routing
{
    [Equals]
    internal class Parameter : ISegmentVariant
    {
        internal Parameter(string key)
        {
            Key = key;
        }

        internal string Key { get; }
    }
}
