using System.Collections.Generic;

namespace Messerli.Routing.Parsing
{
    internal interface IPathParser
    {
        IEnumerable<string>? Parse(string path);
    }
}
