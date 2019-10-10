using System.Collections.Generic;
using Messerli.Routing.Parsing;
using Xunit;

namespace Messerli.Routing.Test
{
    public class PathParserTest
    {
        [Theory]
        [MemberData(nameof(NormalizedPaths))]
        public void PathParserTrimsOneTrailingSegmentDelimiter(IEnumerable<string> expectedSegments, string path)
        {
            var pathParser = new PathParser();
            Assert.Equal(expectedSegments, pathParser.Parse(path));
        }

        public static TheoryData<IEnumerable<string>, string> NormalizedPaths()
        {
            return new TheoryData<IEnumerable<string>, string>
            {
                { new[] { "/" }, "/" },
                { new[] { "/", "foo" }, "/foo" },
                { new[] { "/", "foo", string.Empty }, "/foo//" },
                { new[] { "/", "foo", string.Empty, string.Empty }, "/foo///" },
            };
        }
    }
}
