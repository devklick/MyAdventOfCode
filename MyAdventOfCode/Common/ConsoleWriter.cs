using System.IO;

using Xunit.Abstractions;

namespace MyAdventOfCode.Common;
public class ConsoleWriter : StringWriter
{
    private readonly ITestOutputHelper _output;
    public ConsoleWriter(ITestOutputHelper output)
    {
        _output = output;
    }

    public override void WriteLine(string m)
    {
        _output.WriteLine(m);
    }
}
