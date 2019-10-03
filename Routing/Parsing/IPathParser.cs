using System.Collections.Generic;

namespace Routing.Parsing
{
    public interface IPathParser
    {
        IEnumerable<string>? Parse(string path);
    }
}
