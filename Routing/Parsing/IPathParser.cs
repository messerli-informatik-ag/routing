using System.Collections.Generic;

namespace Messerli.Routing.Parsing
{
    public interface IPathParser
    {
        IEnumerable<string>? Parse(string path);
    }
}
