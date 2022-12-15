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
        => await Invoke(Part.One, DataType.Example, 31);

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke(Part.One, DataType.Actual);

    [Fact]
    public override async Task Part2_Example()
        => await Invoke(Part.Two, DataType.Example);

    [Fact]
    public override async Task Part2_Actual()
        => await Invoke(Part.Two, DataType.Actual);

    private async Task Invoke(Part part, DataType dataType, int? expected = null)
    {
        var data = await GetData(dataType);

        // My WIP solution
        // var map = HeightMap.Parse(data);
        // var routes = map.GetRoute();
        // var result = routes.Count;

        // micka190's solution
        var map = new Cheat.HeightMap(data);
        var route = Cheat.BreadthFirstSearch.Search(map, map.Start, map.End);
        var result = route.Count;

        WriteResult(part, result);

        if (expected != null)
        {
            result.Should().Be(expected);
        }
    }

    private class MapCell
    {
        public RCVector Position { get; }
        public virtual char Display { get; }
        public int Height { get; }
        public bool IsStart => Display == 'S';
        public bool IsDestination => Display == 'E';

        private static readonly Dictionary<char, int> GradientMap;
        static MapCell()
        {
            GradientMap = Enumerable.Range('a', 26)
                .Select((c, i) => new { c, i = i + 1 })
                .ToDictionary(x => Convert.ToChar(x.c), x => x.i);

            GradientMap.Add('S', 0);
            GradientMap.Add('E', GradientMap['z'] + 1);
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
        public List<List<MapCell>> Cells { get; set; }
        public HeightMap(List<List<MapCell>> cells)
        {
            Cells = cells;
        }

        public static HeightMap Parse(string[] input)
            => new(input.Select((line, row) => line.Select((display, col) => new MapCell(display, row, col)).ToList()).ToList());

        public List<MapCell> GetRoute()
        {
            var start = FindCell(cell => cell.IsStart);
            var end = FindCell(cell => cell.IsDestination);

            // keep track of an expanding ring
            var frontier = new Queue<MapCell>(new[] { start });

            // keep track of where we came from for every location that’s been reached
            var cameFrom = new Dictionary<MapCell, MapCell> { { start, null } };

            while (frontier.Any())
            {
                var current = frontier.Dequeue();

                if (current == end)
                {
                    break;
                }

                foreach (var next in GetNeighbors(current, cameFrom))
                {
                    frontier.Enqueue(next);
                    cameFrom[next] = current;
                }
            }

            return BuildRoute(start, end, cameFrom);
        }

        private static List<MapCell> BuildRoute(MapCell start, MapCell end, Dictionary<MapCell, MapCell> cameFrom)
        {
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
