using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAdventOfCode.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2015
{
    public class D10
    {
        private readonly ITestOutputHelper _output;
        public D10(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Part_1_Examples()
        {
            var start = "1";
            var iterations = 5;
            var expectedValues = new[] { "1", "11", "21", "1211", "111221", "312211" };

            var result = Sequencer.CreateSequence(start, 5);
            for (var i = 0; i < iterations; i++)
            {
                Assert.Equal(expectedValues[i], result[i]);
            }
        }

        [Fact]
        public void Part_1()
        {
            var iterations = 40;
            Test_Part_X(iterations);
        }

        [Fact]
        public void Part_2()
        {
            var iterations = 50;
            Test_Part_X(iterations);
        }

        private void Test_Part_X(int iterations)
        {
            var input = "3113322113";
            var sequence = Sequencer.CreateSequence(input, iterations);
            var result = sequence.Last().Length;
            _output.WriteLine(result);
        }

        private class Sequencer
        {
            public static List<string> CreateSequence(string input, int iterations)
            {
                var sequence = new List<string>();
                var current = input;
                for (var i = 0; i <= iterations; i++)
                {
                    var next = new StringBuilder();

                    var charCount = 1;
                    for (var c = 0; c < current.Length; c++)
                    {
                        char currentChar = current[c];

                        var nextCharIndex = c + 1;
                        char? nextChar = nextCharIndex < current.Length ? current[nextCharIndex] : null;

                        if (nextChar.HasValue && nextChar.Value == currentChar)
                        {
                            charCount++;
                        }
                        if (!nextChar.HasValue || nextChar.Value != currentChar)
                        {
                            next.Append(charCount).Append(currentChar);
                            charCount = 1;
                        }

                    }
                    sequence.Add(current);
                    current = next.ToString();
                }
                return sequence;
            }
        }
    }
}