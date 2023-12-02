using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using MyAdventOfCode.Common;

using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2023.D02;

public class Solution : TestBase
{
    public Solution(ITestOutputHelper output) : base(output)
    { }

    [Fact]
    public override async Task Part1_Example()
        => await Invoke(
            Part.One,
            DataType.Example,
            new GameConfiguration(red: 12, green: 13, blue: 14),
            8);

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke(
            Part.One,
            DataType.Actual,
            new GameConfiguration(red: 12, green: 13, blue: 14),
            2486);

    [Fact]
    public override async Task Part2_Example()
        => await Invoke(
            Part.Two,
            DataType.Example,
            new GameConfiguration(red: 12, green: 13, blue: 14));

    [Fact]
    public override async Task Part2_Actual()
        => await Invoke(
            Part.Two,
            DataType.Actual,
            new GameConfiguration(red: 12, green: 13, blue: 14));

    record Game(int Num, IReadOnlyList<Set> Sets);

    record Set(int Red, int Green, int Blue);

    private async Task Invoke(Part part, DataType dataType, GameConfiguration config, int? expected = null)
    {
        var data = await GetData(dataType, part);
        var games = Games.Parse(data, config);
        var validGames = games.GetValidGameResults();
        var result = validGames.Sum(g => g.GameId);

        WriteResult(part, result);

        if (expected != null)
        {
            result.Should().Be(expected, part.ToString());
        }
    }

    private enum GamePiece { Red, Green, Blue };

    private class GameHand
    {
        public Dictionary<GamePiece, int> GamePieces = new();
        public bool IsValid { get; }

        public GameHand(Dictionary<GamePiece, int> gamePieces, bool isValid)
        {
            GamePieces = gamePieces;
            IsValid = isValid;
        }

        public static GameHand Parse(string data, GameConfiguration config)
        {
            var results = new Dictionary<GamePiece, int>();
            foreach (var gamePieceInfo in data.Split(','))
            {
                var parts = gamePieceInfo.Trim().Split(' ');
                var count = int.Parse(parts.First().Trim());
                var gamePiece = ParseGamePiece(parts.Last().Trim());
                results.Add(gamePiece, count);
            }
            var valid = results.All(r => r.Value <= config.Totals[r.Key]);
            return new(results, valid);
        }

        private static GamePiece ParseGamePiece(string value) => value switch
        {
            "blue" => GamePiece.Blue,
            "red" => GamePiece.Red,
            "green" => GamePiece.Green,
            _ => throw new NotImplementedException("Invalid game piece " + value)
        };
    }

    private class GameResult
    {
        public int GameId { get; }
        public bool IsValid { get; }

        private readonly List<GameHand> _hands;

        public GameResult(int gameId, List<GameHand> hands)
        {
            GameId = gameId;
            _hands = hands;
            IsValid = _hands.All(h => h.IsValid);
        }

        public static GameResult Parse(string data, GameConfiguration config)
        {
            var id = ParseGameId(data);
            var hands = ParseHands(data, config);

            return new(id, hands);
        }

        private static int ParseGameId(string data)
            => int.Parse(data.Split(':').First().Split(' ').Last());

        private static List<GameHand> ParseHands(string data, GameConfiguration config)
            => data.Split(':').Last().Split(';').Select(x => GameHand.Parse(x, config)).ToList();
    }

    private class Games
    {
        private readonly List<GameResult> _results;

        public Games(List<GameResult> results)
        {
            _results = results;
        }

        public IEnumerable<GameResult> GetValidGameResults()
            => _results.Where(r => r.IsValid);

        public static Games Parse(string[] lines, GameConfiguration config)
        {
            var results = new List<GameResult>();

            foreach (var line in lines)
            {
                results.Add(GameResult.Parse(line.Trim(), config));
            }

            return new Games(results);
        }
    }

    private class GameConfiguration
    {
        public IReadOnlyDictionary<GamePiece, int> Totals { get; }

        public GameConfiguration(int red, int green, int blue)
        {
            var totals = new Dictionary<GamePiece, int>
            {
                [GamePiece.Red] = red,
                [GamePiece.Green] = green,
                [GamePiece.Blue] = blue
            };

            Totals = totals;
        }
    }
}