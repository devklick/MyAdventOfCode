using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using MyAdventOfCode.Common;

using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2023.D01;

public class Solution : TestBase
{
    public Solution(ITestOutputHelper output) : base(output)
    { }

    [Fact]
    public override async Task Part1_Example()
        => await Invoke(Part.One, DataType.Example, 142);

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke(Part.One, DataType.Actual, 54916);

    [Fact]
    public override async Task Part2_Example()
        => await Invoke(Part.Two, DataType.Example, 281);

    [Fact]
    public override async Task Part2_Actual()
        => await Invoke(Part.Two, DataType.Actual, 54728);

    private async Task Invoke(Part part, DataType dataType, int? expected = null)
    {
        var data = await GetData(dataType, part);
        var parseNumberNames = part == Part.Two;
        var doc = new CalibrationDocument(data, parseNumberNames);

        var result = doc.GetTotalCalibrationValue();

        WriteResult(part, result);

        if (expected != null)
        {
            result.Should().Be(expected, part.ToString());
        }
    }

    private class CalibrationDocument
    {
        private readonly IEnumerable<CalibrationText> _lines;
        public CalibrationDocument(string[] lines, bool parseNumberNames)
        {
            _lines = lines.Select(line => new CalibrationText(line, parseNumberNames));
        }

        public int GetTotalCalibrationValue()
            => _lines.Sum(line => line.GetCalibrationValue());
    }

    private class CalibrationText
    {
        private readonly string _text;
        private readonly IEnumerable<char> _digits;
        private readonly Dictionary<string, char> _numberNames = new()
        {
            { "one", '1'},
            { "two", '2'},
            { "three", '3'},
            { "four", '4'},
            { "five", '5'},
            { "six", '6'},
            { "seven", '7'},
            { "eight", '8'},
            { "nine", '9'},
        };

        public CalibrationText(string text, bool parseNumberNames)
        {
            _text = text;
            _digits = GetDigits(parseNumberNames);
        }

        public int GetCalibrationValue()
            => int.Parse($"{_digits.First()}{_digits.Last()}");

        private IEnumerable<char> GetDigits(bool parseNumberNames)
        {
            if (!parseNumberNames)
            {
                return _text.Where(char.IsDigit);
            }

            var digits = new List<char>();

            for (var i = 0; i < _text.Length; i++)
            {
                var character = _text[i];

                if (char.IsDigit(character))
                {
                    digits.Add(character);
                    continue;
                }

                var numberFromName = ParseForNumberName(i);

                if (numberFromName.HasValue)
                {
                    digits.Add(numberFromName.Value);
                    continue;
                }
            }

            return digits;
        }

        private char? ParseForNumberName(int start)
        {
            var numberName = string.Empty;
            for (var i = start; i < _text.Length; i++)
            {
                numberName = string.Concat(numberName, _text[i]);
                var candidates = _numberNames.Where(n => n.Key.StartsWith(numberName));

                if (!candidates.Any())
                {
                    return null;
                }

                if (candidates.Count() == 1 && candidates.First().Key == numberName)
                {
                    return candidates.First().Value;
                }
            }
            return null;
        }
    }
}