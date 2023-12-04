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
            (cards) => cards.Sum(c => c.Value.PointsWon),
            13);

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke(
            Part.One,
            DataType.Actual,
            (cards) => cards.Sum(c => c.Value.PointsWon),
            23847);

    [Fact]
    public override async Task Part2_Example()
        => await Invoke(
            Part.Two,
            DataType.Example,
            (cards) => cards.GetWinningCards().Sum(c => cards.GetPrizesForCard(c).Count()) + cards.Count,
            30);

    [Fact]
    public override async Task Part2_Actual()
        => await Invoke(
            Part.Two,
            DataType.Actual,
            (cards) => cards.GetWinningCards().Sum(c => cards.GetPrizesForCard(c).Count()) + cards.Count,
            8570000);

    private async Task Invoke(Part part, DataType dataType, Func<ScratchCardCollection, int> getResult, int? expected = null)
    {
        var data = await GetData(dataType, part);
        var cards = new ScratchCardCollection(ScratchCard.ParseMany(data));
        var result = getResult(cards);

        WriteResult(part, result);

        if (expected != null)
        {
            result.Should().Be(expected, part.ToString());
        }
    }

    private class ScratchCardCollection : Dictionary<int, ScratchCard>
    {
        public ScratchCardCollection(IEnumerable<ScratchCard> cards)
            : base(cards.ToDictionary(key => key.Id, value => value))
        { }

        public IEnumerable<ScratchCard> GetWinningCards()
            => Values.Where(c => c.MatchedNumbers.Any());

        public IEnumerable<ScratchCard> GetPrizesForCard(ScratchCard card)
            => GetPrizesForCard(card, card.Id);

        private IEnumerable<ScratchCard> GetPrizesForCard(ScratchCard card, int sourceCardId)
        {
            // recursive yield is EXTREMELY slow. 
            // There's probably a much better way of doing this, 
            // but I've reached this point and got the right answer 
            // so I'm not gonna refactor it
            foreach (var prize in GetCardSequence(card.Id + 1, card.MatchedNumbers.Count()))
            {
                yield return prize;

                foreach (var childPrize in GetPrizesForCard(prize, sourceCardId))
                {
                    yield return childPrize;
                }
            }
        }

        private IEnumerable<ScratchCard> GetCardSequence(int startCardId, int count)
        {
            for (var i = startCardId; i < startCardId + count; i++)
            {
                yield return this[i];
            }
        }
    }

    partial class ScratchCardNumber
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

    private class ScratchCard
    {
        public int Id { get; }
        public IEnumerable<ScratchCardNumber> WinningNumbers { get; }
        public IEnumerable<int> PlayerNumbers { get; }
        public IEnumerable<ScratchCardNumber> MatchedNumbers => WinningNumbers.Where(n => PlayerNumbers.Contains(n.Value));
        public int PointsWon { get; }

        public ScratchCard(int id, IEnumerable<ScratchCardNumber> winningNumbers, IEnumerable<int> playerNumbers)
        {
            Id = id;
            WinningNumbers = winningNumbers;
            PlayerNumbers = playerNumbers;
            PointsWon = GetPointsWon();
        }

        private int GetPointsWon()
            => MatchedNumbers.Aggregate(0, (total, current) => total == 0 ? 1 : total * 2);

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