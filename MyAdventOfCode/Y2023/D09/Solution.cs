using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using MyAdventOfCode.Common;
using MyAdventOfCode.Extensions;

using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2023.D09;

public partial class Solution : TestBase
{
    private static ConsoleWriter Writer;
    public Solution(ITestOutputHelper output) : base(output)
    {
        Writer = new ConsoleWriter(_output);
    }

    [Fact]
    public override async Task Part1_Example()
        => await Invoke_Part1(
            DataType.Example,
            114);

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke_Part1(
            DataType.Actual,
            1782868781);

    [Fact]
    public override async Task Part2_Example()
        => await Invoke_Part2(
            DataType.Example,
            2);

    [Fact]
    public override async Task Part2_Actual()
        => await Invoke_Part2(
            DataType.Actual,
            1057);

    private async Task Invoke_Part1(DataType dataType, int? expected = null)
        => await Invoke(Part.One, dataType, Direction.Forward, expected);

    private async Task Invoke_Part2(DataType dataType, int? expected = null)
        => await Invoke(Part.Two, dataType, Direction.Backward, expected);

    private async Task Invoke(Part part, DataType dataType, Direction direction, int? expected = null)
    {
        var data = await GetData(dataType, part);
        var report = OSIReport.Parse(data);
        var result = report.SumPredictions(direction);

        WriteResult(part, result);

        if (expected != null)
        {
            result.Should().Be(expected, part.ToString());
        }
    }

    private enum Direction { Forward, Backward };

    private class OSIReport
    {
        private readonly List<OSIReportValueHistory> _histories;

        public OSIReport(IEnumerable<OSIReportValueHistory> histories)
        {
            _histories = histories.ToList();
        }

        public static OSIReport Parse(string[] data)
            => new(data.Select(OSIReportValueHistory.Parse));

        public int SumPredictions(Direction extrapolationDirection)
            => _histories.Sum(h => h.PredictNext(extrapolationDirection));
    }

    private class OSIReportValueHistory
    {
        private readonly List<List<int>> _values;
        private bool LastAllZeros => _values.Last().All(v => v == 0);

        public OSIReportValueHistory(IEnumerable<int> values)
        {
            _values = new List<List<int>> { values.ToList() };
        }

        public static OSIReportValueHistory Parse(string data)
            => new(data.Split().Select(int.Parse));

        public int PredictNext(Direction extrapolationDirection)
        {
            while (!LastAllZeros) GenerateDiffs();

            return Extrapolate(extrapolationDirection);
        }

        private void GenerateDiffs()
            => _values.Add(_values.Last().Aggregate(new List<int>(), (res, val, i) =>
            {
                if (i + 1 < _values.Last().Count) res.Add(_values.Last()[i + 1] - val);
                return res;
            }));

        private int Extrapolate(Direction direction)
        {
            for (var i = _values.Count - 1; i >= 0; i--)
            {
                var value = i == _values.Count - 1
                    ? 0
                    : direction == Direction.Forward
                        ? _values[i].Last() + _values[i + 1].Last()
                        : _values[i].First() - _values[i + 1].First();

                if (direction == Direction.Forward) _values[i].Add(value);
                else _values[i].Insert(0, value);
            }

            return direction == Direction.Forward
                ? _values.First().Last()
                : _values.First().First();
        }
    }
}
