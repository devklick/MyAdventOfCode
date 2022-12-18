using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using MyAdventOfCode.Common;
using MyAdventOfCode.Common.Attributes;
using MyAdventOfCode.Extensions;

using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2022.D12;

public class Solution : TestBase
{
    public Solution(ITestOutputHelper output) : base(output)
    { }

    [Fact]
    public override async Task Part1_Example()
        => await Invoke_Part1(DataType.Example, 31);

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke_Part1(DataType.Actual, 517);

    [Fact]
    public override async Task Part2_Example()
        => await Invoke_Part2(DataType.Example, 29);

    [Fact]
    public override async Task Part2_Actual()
        => await Invoke_Part2(DataType.Actual, 512);

    private async Task Invoke_Part1(DataType dataType, int? expected = null)
        => await Invoke(Part.One, dataType, map => map.GetRoute().Count, expected);

    private async Task Invoke_Part2(DataType dataType, int? expected = null)
    {
        await Invoke(Part.Two, dataType, map =>
        {
            List<MapCell> result = null;
            for (int row = 0; row < map.Height; row++)
            {
                for (int col = 0; col < map.Width; col++)
                {
                    var start = map.Cells[row][col];
                    if (!map.IsOnPerimeter(start)) continue;
                    if (start.Display != MapCell.LowestElevationMarker) continue;

                    var candidate = map.GetRoute(start);
                    if (result == null || (candidate.Count > 0 && candidate.Count < result.Count))
                    {
                        result = candidate;
                    }
                }
            }
            return result.Count;
        }, expected);
    }

    private async Task Invoke(Part part, DataType dataType, Func<HeightMap, int> sut, int? expected = null)
    {
        var data = await GetData(dataType);
        var map = HeightMap.Parse(data);
        var result = sut.Invoke(map);

        WriteResult(part, result);

        if (expected != null)
        {
            result.Should().Be(expected);
        }
    }

    private class MapCell
    {
        public static readonly char StartMarker = 'S';
        public static readonly char DestinationMarker = 'E';
        public static readonly char LowestElevationMarker = 'a';
        public static readonly char HighestElevationMarker = 'z';

        public RCVector Position { get; }
        public virtual char Display { get; }
        public int Height { get; }
        public bool IsStart => Display == StartMarker;
        public bool IsDestination => Display == DestinationMarker;

        private static readonly Dictionary<char, int> GradientMap;
        static MapCell()
        {
            GradientMap = Enumerable.Range(LowestElevationMarker, 26)
                .Select((c, i) => new { c, i })
                .ToDictionary(x => Convert.ToChar(x.c), x => x.i);

            GradientMap.Add(StartMarker, GradientMap[LowestElevationMarker]);
            GradientMap.Add(DestinationMarker, GradientMap[HighestElevationMarker]);
        }

        public MapCell(char display, int row, int column)
        {
            Display = display;
            Height = GradientMap[display];
            Position = new RCVector(row, column);
        }
    }

    private class HeightMap
    {
        public int Height => Cells.Count;
        public int Width => Cells.FirstOrDefault()?.Count ?? 0;
        public List<List<MapCell>> Cells { get; set; }
        public HeightMap(List<List<MapCell>> cells)
        {
            Cells = cells;
        }

        public static HeightMap Parse(string[] input)
            => new(input.Select((line, row) => line.Select((display, col) => new MapCell(display, row, col)).ToList()).ToList());

        public List<MapCell> GetRoute(MapCell startAt = null)
        {
            var start = startAt ?? FindCell(cell => cell.IsStart);
            var end = FindCell(cell => cell.IsDestination);

            // keep track of an expanding ring
            var frontier = new Queue<MapCell>(new[] { start });

            // keep track of where we came from for every location that’s been reached
            var cameFrom = new Dictionary<MapCell, MapCell> { { start, null } };

            while (frontier.Any())
            {
                var current = frontier.Dequeue();

                if (current == end) break;

                foreach (var next in GetNeighbors(current, cameFrom))
                {
                    frontier.Enqueue(next);
                    cameFrom[next] = current;
                }
            }

            return BuildRoute(start, end, cameFrom);
        }

        public bool IsOnPerimeter(MapCell cell)
        {
            var inner = cell.Position.Row >= 1 && cell.Position.Row <= Height - 2 && cell.Position.Column >= 1 && cell.Position.Column <= Width - 2;
            return !inner;
        }

        private static List<MapCell> BuildRoute(MapCell start, MapCell end, Dictionary<MapCell, MapCell> cameFrom)
        {
            if (!cameFrom.ContainsKey(end)) return new();

            var current = end;
            var path = new List<MapCell>();

            while (current != start)
            {
                path.Add(current);
                current = cameFrom[current];
            }

            path.Reverse();

            return path;
        }

        private MapCell FindCell(Func<MapCell, bool> predicate)
            => Cells.AggregateUntil(
                (MapCell)null,
                (acc, cur) => cur.FirstOrDefault(c => predicate.Invoke(c)),
                value => value != null);

        private IEnumerable<MapCell> GetNeighbors(MapCell current, Dictionary<MapCell, MapCell> ignore)
        {
            foreach (var position in RCVector.GetNeighbors(current.Position, RCVector.Cardinals))
            {
                var neighbor = Cells.ElementAtOrDefault(position.Row)?.ElementAtOrDefault(position.Column);
                if (neighbor == null) continue;

                if (ignore.ContainsKey(neighbor)) continue;
                if (neighbor.Height - current.Height <= 1) yield return neighbor;
            }
        }
    }
}
