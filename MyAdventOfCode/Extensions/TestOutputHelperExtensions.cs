using Xunit.Abstractions;

namespace MyAdventOfCode.Extensions
{
    public static class TestOutputHelperExtensions
    {
        public static void WriteLine(this ITestOutputHelper helper, object valueToOutput)
            => helper.WriteLine(valueToOutput?.ToString());
    }
}
