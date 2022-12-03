using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using MyAdventOfCode.Common;

using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2022.D02;

public class Solution : TestBase
{
    protected override int DayNo => 2;
    protected override int YearNo => 2022;

    public Solution(ITestOutputHelper output) : base(output)
    { }

    [Fact]
    public override async Task Part1_VerifyExample()
        => await Invoke(Part.One, DataType.Example, 2, 15);

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke(Part.One, DataType.Actual, 2, 14827); // 14827

    [Fact]
    public override async Task Part2_VerifyExample()
        => await Invoke(Part.Two, DataType.Example, 2, 12);

    [Fact]
    public override async Task Part2_Actual()
        => await Invoke(Part.Two, DataType.Actual, 2, 13889); // 13889

    private async Task Invoke(Part part, DataType dataType, int playerNo, int? expected = null)
    {
        var data = await GetData(dataType);
        var game = new Game(data, part);
        var scores = game.CalcScores();
        var result = scores.First(s => s.PlayerNo == playerNo).Score;

        WriteResult(part, result);

        if (expected != null)
        {
            result.Should().Be(expected, part.ToString());
        }
    }

    private abstract class Outcome
    {
        public static Win Win = new();
        public static Draw Draw = new();
        public static Lose Lose = new();
        private static readonly List<Outcome> All = new() { Win, Draw, Lose };
        public static Outcome Parse(char c) => All.First(o => o.Char == c);

        public abstract int Bonus { get; }
        public abstract char Char { get; }
    }

    private class Win : Outcome
    {
        public override int Bonus => 6;
        public override char Char => 'Z';
    }
    private class Draw : Outcome
    {
        public override int Bonus => 3;
        public override char Char => 'Y';
    }

    private class Lose : Outcome
    {
        public override int Bonus => 0;
        public override char Char => 'X';
    }

    private abstract class Hand
    {
        private static readonly List<Hand> All = new() { new Rock1(), new Paper1(), new Scissor1(), new Rock2(), new Paper2(), new Scissor2() };
        public static Hand Parse(char handChar, Part part) => All.FirstOrDefault(h => h.Chars.Any(c => c == handChar && h.GamePart == part));
        public static Hand ParseByOutcome(Outcome desiredOutcome, List<PlayerHand> opponentsHands, Part part) =>
             desiredOutcome switch
             {
                 Win => All.First(h => opponentsHands.Any(o => o.Hand.DefeatedByType == h.Type && part == h.GamePart)),
                 Draw => All.First(h => h.Type == opponentsHands.First().Hand.Type && part == h.GamePart),
                 Lose => All.First(h => opponentsHands.Any(o => h.DefeatedByType == o.Hand.Type) && h.GamePart == part),
                 _ => throw new NotImplementedException("Outcome not supported")
             };

        public enum Types { Rock, Paper, Scissor };

        public abstract List<char> Chars { get; }
        public abstract int Points { get; }
        public abstract Types Type { get; }
        public abstract Types DefeatedByType { get; }
        public abstract Part GamePart { get; }
    }

    private class Rock1 : Hand
    {
        public override List<char> Chars => new() { 'A', 'X' };
        public override int Points => 1;
        public override Types Type => Types.Rock;
        public override Types DefeatedByType => Types.Paper;
        public override Part GamePart => Part.One;
    }

    private class Rock2 : Rock1
    {
        public override List<char> Chars => new() { 'A' };
        public override Part GamePart => Part.Two;
    }

    private class Paper1 : Hand
    {
        public override List<char> Chars => new() { 'B', 'Y' };
        public override int Points => 2;
        public override Types Type => Types.Paper;
        public override Types DefeatedByType => Types.Scissor;
        public override Part GamePart => Part.One;
    }

    private class Paper2 : Paper1
    {
        public override List<char> Chars => new() { 'B' };
        public override Part GamePart => Part.Two;
    }

    private class Scissor1 : Hand
    {
        public override List<char> Chars => new() { 'C', 'Z' };
        public override int Points => 3;
        public override Types Type => Types.Scissor;
        public override Types DefeatedByType => Types.Rock;
        public override Part GamePart => Part.One;
    }

    private class Scissor2 : Scissor1
    {
        public override List<char> Chars => new() { 'C' };
        public override Part GamePart => Part.Two;
    }

    private class Game
    {
        private readonly List<Round> _rounds = new();

        public Game(string[] data, Part part)
        {
            foreach (var line in data)
            {
                var handChars = line.Split(' ').Select(v => v.ToCharArray().First());
                _rounds.Add(new Round(handChars, part));
            }
        }

        public IEnumerable<PlayerScore> CalcScores()
        {
            var playerScores = new Dictionary<int, int>();
            foreach (var round in _rounds)
            {
                foreach (var playerScore in round.CalcScores())
                {
                    playerScores.TryGetValue(playerScore.PlayerNo, out var total);
                    playerScores[playerScore.PlayerNo] = total + playerScore.Score;
                }
            }

            return playerScores.Select(d => new PlayerScore(d.Key, d.Value));
        }
    }

    private class Round
    {
        private readonly List<PlayerHand> _playerHands = new();

        public Round(IEnumerable<char> hands, Part part)
        {
            var playerNo = 1;
            foreach (var h in hands)
            {
                var hand = Hand.Parse(h, part);

                if (hand == null && part == Part.Two)
                {
                    var outcome = Outcome.Parse(h);
                    hand = Hand.ParseByOutcome(outcome, _playerHands, part);
                }

                _playerHands.Add(new PlayerHand(playerNo, hand));

                playerNo++;
            }
        }

        public IEnumerable<PlayerScore> CalcScores() => _playerHands.Select((playerHand, index) =>
        {
            var draw = _playerHands.Where((other, otherIndex) => otherIndex != index && other.Hand == playerHand.Hand).Any();
            var win = !draw && _playerHands.Where((other, otherIndex) => index != otherIndex && other.Hand.DefeatedByType == playerHand.Hand.Type).Any();
            var loss = !win && !draw;

            var points = playerHand.Hand.Points + (draw ? Outcome.Draw.Bonus : win ? Outcome.Win.Bonus : Outcome.Lose.Bonus);
            return new PlayerScore(playerHand.PlayerNo, points);
        });
    }

    private class PlayerHand
    {
        public int PlayerNo { get; set; }
        public Hand Hand { get; set; }
        public PlayerHand(int playerNo, Hand hand)
        {
            PlayerNo = playerNo;
            Hand = hand;
        }
    }

    public class PlayerScore
    {
        public int PlayerNo { get; set; }
        public int Score { get; set; }
        public PlayerScore(int playerNo, int score)
        {
            PlayerNo = playerNo;
            Score = score;
        }
    }
}