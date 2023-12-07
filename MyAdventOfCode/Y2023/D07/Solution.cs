using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using MyAdventOfCode.Common;

using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2023.D07;

public class Solution : TestBase
{
    public Solution(ITestOutputHelper output) : base(output)
    { }

    [Fact]
    public override async Task Part1_Example()
        => await Invoke(
            Part.One,
            DataType.Example,
            null,
            null,
            6440);

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke(
            Part.One,
            DataType.Actual,
            null,
            null,
            248113761);

    [Fact]
    public override async Task Part2_Example()
        => await Invoke(
            Part.Two,
            DataType.Example,
            new(){ { 'J', 0 } }, // J now lowest value
            new List<char>{'J'}, // J now wildcard
            5905);

    [Fact]
    public override async Task Part2_Actual()
    => await Invoke(
        Part.Two,
        DataType.Actual,
        new(){ { 'J', 0 } }, // J now lowest value
        new List<char>{'J'}, // J now wildcard
        21039729);

    [Fact]
    public void HighCard_OneJ_Pair()
        => VerifyWildcardHandType("2345J", HandType.OnePair);
    [Fact]
    public void HighCard_TwoJ_ThreeOfKind()
        => VerifyWildcardHandType("234JJ", HandType.ThreeOfAKind);

    [Fact]
    public void HighCard_TwoJ_FourOfKind()
        => VerifyWildcardHandType("23JJJ", HandType.FourOfAKind);

    [Fact]
    public void HighCard_TwoJ_FiveOfKind()
        => VerifyWildcardHandType("2JJJJ", HandType.FiveOfAKind);

    [Fact]
    public void AllJ_FiveOfKind()
        => VerifyWildcardHandType("JJJJJ", HandType.FiveOfAKind);

    [Fact]
    public void Pair_OneJ_ThreeOfKind()
        => VerifyWildcardHandType("2234J", HandType.ThreeOfAKind);

    [Fact]
    public void Pair_TwoJ_FourOfKind()
        => VerifyWildcardHandType("223JJ", HandType.FourOfAKind);

    [Fact]
    public void Pair_ThreeJ_FiveOfKind()
        => VerifyWildcardHandType("22JJJ", HandType.FiveOfAKind);

    [Fact]
    public void TwoPair_OneJ_FiveOfKind()
        => VerifyWildcardHandType("2233J", HandType.FullHouse);

    [Fact]
    public void ThreeOfKind_OneJ_FourOfKind()
        => VerifyWildcardHandType("2223J", HandType.FourOfAKind);

    [Fact]
    public void ThreeOfKind_TwoJ_FiveOfKind()
        => VerifyWildcardHandType("222JJ", HandType.FiveOfAKind);

    [Fact]
    public void ThreeOfKind_TwoJ_FourOfKind()
        => VerifyWildcardHandType("222JJ", HandType.FiveOfAKind);

    [Fact]
    public void FullHouse_ZeroJ_FullHouse()
        => VerifyWildcardHandType("22233", HandType.FullHouse);

    private static void VerifyWildcardHandType(string cards, HandType expected)
    {
        var factory = new CardFactory(new(){ { 'J', 0 } });
        var hand = new Hand(cards.Select(factory.CreateCard).ToList(), 0, new(){ 'J' });
        hand.Type.Should().Be(expected);

    }

    private async Task Invoke(Part part, DataType dataType, Dictionary<char, int> cardValueOverrides = null, List<char> wildCards = null, long? expected = null)
    {
        var data = await GetData(dataType, part);
        var cardFactory = new CardFactory(cardValueOverrides);
        var hands = Hand.ParseMany(data, cardFactory, wildCards);
        var game = new Game(hands);
        var result = (long)game.CalculateWinnings();

        WriteResult(part, result);

        if (expected != null)
        {
            result.Should().Be(expected, part.ToString());
        }
    }

    private class Card : IComparable<Card>
    {
        public char Type { get; }
        public int Value { get; }
        public Card(char type, int value)
        {
            Type = type;
            Value = value;
        }

        public int CompareTo(Card other)
            => Value.CompareTo(other.Value);
    }

    private class CardFactory
    {
        private readonly Dictionary<char, int> _cardValues = new()
        {
            { '2', 1},
            { '3', 2},
            { '4', 3},
            { '5', 4},
            { '6', 5},
            { '7', 6},
            { '8', 7},
            { '9', 8},
            { 'T', 9},
            { 'J', 10},
            { 'Q', 11},
            { 'K', 12},
            { 'A', 13},
        };

        public CardFactory(Dictionary<char, int> cardValueOverrides = null)
        {
            if (cardValueOverrides == null) return;
            foreach(var card in cardValueOverrides)
            {
                _cardValues[card.Key] = card.Value;
            }
        }

        public Card CreateCard(char type) => new(type, _cardValues[type]);
    }

    private class Hand
    {
        private readonly Dictionary<HandType, int[]> _handTypeMapping = new()
        {
            { HandType.FiveOfAKind, new[]{5}},
            { HandType.FourOfAKind, new[]{4}},
            { HandType.FullHouse, new[]{2,3}},
            { HandType.ThreeOfAKind, new[]{3}},
            { HandType.TwoPair, new[]{2, 2}},
            { HandType.OnePair, new[]{2}},
            { HandType.HighCard, new[]{0}}
        };

        public List<Card> Cards { get; }
        public int Bid { get; }
        public HandType Type { get; }
        public Hand(List<Card> cards, int bid, List<char> wildCards)
        {
            Cards = cards;
            Bid = bid;
            Type = DetermineHandType(wildCards);
        }
        public override string ToString() => $"{string.Join("", Cards.Select(c => c.Type))} {Bid} {Type}";

        private HandType DetermineHandType(List<char> wildCards)
        {
            foreach(var mapping in _handTypeMapping)
            {                
                if (MeetsSameTypeCriteria(mapping.Value, wildCards))
                {
                    return mapping.Key;
                }
            }
            
            return HandType.HighCard;
        }

        private bool MeetsSameTypeCriteria(int[] sameTypeCounts, List<char> wildCards)
        {
            // This has ended up super messy, and definitely would not work
            // if more, different wildcards get introduced. It's brittle, 
            // but it works for what's currently needed, so it's staying.

            var groups = Cards
                .GroupBy(c => c.Type)
                .Select(g => new { Type = g.Key, g.ToList().Count })
                .OrderBy(x => x.Count);

            var typesUsed = new List<char>();
            var wildCardsUsed =new List<char>();
            
            foreach (var typeCount in sameTypeCounts.OrderBy(x => x))
            {
                var wildCardMatches = wildCards?.Count > 0 && wildCards.Count > wildCardsUsed.Count
                    ? groups.FirstOrDefault(g => g.Type == wildCards.First())?.Count ?? 0 
                    : 0;

                var match = groups.FirstOrDefault(g => {
                    if (wildCardMatches == typeCount) return true;
                    if (typesUsed.Contains(g.Type)) return false;
                    if (wildCards.Contains(g.Type)) return false;
                    if (g.Count == typeCount) return true;
                    if (g.Count + wildCardMatches == typeCount) return true;
                    return false;
                });

                if (match == null)
                {
                    return false;
                }

                typesUsed.Add(match.Type);
                if (match.Count < typeCount)
                {
                    wildCardsUsed.Add(wildCards.First());
                }
            }

            return true;
        }

        public static List<Hand> ParseMany(string[] data, CardFactory cardFactory, List<char> wildCards)
            => data.Select(d => Parse(d, cardFactory, wildCards)).ToList();
        
        private static Hand Parse(string data, CardFactory cardFactory, List<char> wildCards)
        {
            var parts = data.Split();
            var cards = parts.First().Select(cardFactory.CreateCard).ToList();
            var bid = int.Parse(parts.Last());
            return new(cards, bid, wildCards);
        }
    }

    private enum HandType
    {
        Unknown, 
        HighCard, 
        OnePair, 
        TwoPair,
        ThreeOfAKind, 
        FullHouse, 
        FourOfAKind, 
        FiveOfAKind
    }

    private class Game
    {
        private readonly List<Hand> _hands;
        public Game(List<Hand> hands)
        {
            _hands = hands.OrderBy(hand => hand.Type)
                .ThenBy(hand => hand.Cards[0])
                .ThenBy(hand => hand.Cards[1])
                .ThenBy(hand => hand.Cards[2])
                .ThenBy(hand => hand.Cards[3])
                .ThenBy(hand => hand.Cards[4])
                .ToList();
        }

        public int CalculateWinnings()
            => _hands.Select((hand, handRank) => hand.Bid * (handRank + 1)).Sum();
    }
}
