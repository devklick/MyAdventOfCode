using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using MyAdventOfCode.Common;
using MyAdventOfCode.Extensions;

using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2023.D04;

public class Solution : TestBase
{
    public Solution(ITestOutputHelper output) : base(output)
    { }

    [Fact]
    public override async Task Part1_Example()
        => await Invoke(
            Part.One,
            DataType.Example);

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke(
            Part.One,
            DataType.Actual);

    [Fact]
    public override async Task Part2_Example()
        => await Invoke(
            Part.Two,
            DataType.Example);

    [Fact]
    public override async Task Part2_Actual()
        => await Invoke(
            Part.Two,
            DataType.Actual);

    private async Task Invoke(Part part, DataType dataType, int? expected = null)
    {
        var data = await GetData(dataType, part);
        var result = 0;

        WriteResult(part, result);

        if (expected != null)
        {
            result.Should().Be(expected, part.ToString());
        }
    }

    public class ScratchCardNumber 
    {
        private readonly int _value;
        public bool Revealed { get; }
        public int Value => Revealed ? _value : throw new Exception("Cannot get value as not yet revealed");

        public ScratchCardNumber(int value, bool revealed = true)
        {
            _value = value;
            Revealed = revealed;
        }
    }

    public class ScratchCard
    {
        public int Id { get; }
        public List<ScratchCardNumber> Numbers { get; }
    }

    public class Pick
    {
        public List<int> Numbers { get; }

        // There seems to be a one-to-one relationship between
        // picked numbers and a scratch card, so we only 
        // need one scratch card here for now
        public ScratchCard ScratchCard { get; }

        public IEnumerable<ScratchCardNumber> WinningNumbers => ScratchCard.Numbers.Where(n => Numbers.Contains(n.Value));

        public int PointsWon => WinningNumbers.Aggregate(1, (total, current) => total * 2);
    }
}