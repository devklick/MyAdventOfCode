using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
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
            DataType.Example,
            (cards) => cards.Sum(c => c.PointsWon),
            13);

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke(
            Part.One,
            DataType.Actual,
            (cards) => cards.Sum(c => c.PointsWon),
            23847);

    [Fact]
    public override async Task Part2_Example()
        => await Invoke(
            Part.Two,
            DataType.Example,
            (cards) => cards.Sum(c => c.PointsWon));

    [Fact]
    public override async Task Part2_Actual()
        => await Invoke(
            Part.Two,
            DataType.Actual,
            (cards) => cards.Sum(c => c.PointsWon));

    private async Task Invoke(Part part, DataType dataType, Func<IEnumerable<ScratchCard>, int> getResult, int? expected = null)
    {
        var data = await GetData(dataType, part);
        var cards = ScratchCard.ParseMany(data);
        var result = getResult(cards);

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
        public IEnumerable<ScratchCardNumber> WinningNumbers { get; }
        public IEnumerable<int> PlayerNumbers { get; }
        public IEnumerable<ScratchCardNumber> PlayerWinningNumbers => WinningNumbers.Where(n => PlayerNumbers.Contains(n.Value));
        public int PointsWon => GetPointsWon();

        public ScratchCard(int id, IEnumerable<ScratchCardNumber> winningNumbers, IEnumerable<int> playerNumbers)
        {
            Id = id;
            WinningNumbers = winningNumbers;
            PlayerNumbers = playerNumbers;
        }

        private int GetPointsWon()
        {
            return PlayerWinningNumbers.Aggregate(0, (total, current) =>
            {
                return total == 0 ? 1 : total * 2;
            });
        }

        public static IEnumerable<ScratchCard> ParseMany(string[] data)
            => data.Select(Parse);

        public static ScratchCard Parse(string data)
        {
            var split = data.Split(':');
            var cardId = int.Parse(split.First().Split(' ').Last());
            var (scratchCardNumbers, playerNumbers) = ParseNumbers(split.Last());

            return new(cardId, scratchCardNumbers, playerNumbers);
        }

        private static (IEnumerable<ScratchCardNumber>, IEnumerable<int>) ParseNumbers(string data)
        {
            var split = data.Split('|');
            var scratchCardNumbers = ParseWinningNumbers(split.First().Trim());
            var playerNumbers = ParsePlayerNumbers(split.Last().Trim());
            return (scratchCardNumbers, playerNumbers);
        }

        private static IEnumerable<int> ParsePlayerNumbers(string data)
            => data.Split().Where(d => !string.IsNullOrEmpty(d)).Select(int.Parse);

        private static IEnumerable<ScratchCardNumber> ParseWinningNumbers(string data)
            => data.Split().Where(d => !string.IsNullOrEmpty(d)).Select(n => new ScratchCardNumber(int.Parse(n)));
    }
}