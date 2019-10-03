using System.Collections.Generic;

namespace Routing.Parsing
{
    internal interface IPathParser
    {
        IEnumerable<string>? Parse(string path);
    }
}
