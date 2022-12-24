using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;

using MyAdventOfCode.Common;
using MyAdventOfCode.Common.Attributes;
using MyAdventOfCode.Extensions;

using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2022.D14;

public class Solution : TestBase
{
    private static readonly bool SuppressConsoleWriteLine = true;
    public Solution(ITestOutputHelper output) : base(output, SuppressConsoleWriteLine)
    { }

    [Fact]
    public override async Task Part1_Example()
        => await Invoke(Part.One, DataType.Example, false, 0, 24);

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke(Part.One, DataType.Actual, false, 0, 828);

    [Fact]
    public override async Task Part2_Example()
        => await Invoke(Part.Two, DataType.Example, true, 1, 93);

    [Fact]
    public override async Task Part2_Actual()
        => await Invoke(Part.Two, DataType.Actual, true, 1, 25500);

    private async Task Invoke(Part part, DataType dataType, bool infiniteWidth = false, int heightExtension = 0, int? expected = null)
    {
        var data = await GetData(dataType);
        // The log window ensures that if logging is enabled, only cells who's position
        // is within the bounds of the rectangle ge logged to the console. 
        // This helps with debugging the example, trying to match the same output shown 
        var logWindow = new Rectangle(494 - 10, 0, 503 + 10, 9 + heightExtension);
        var sandEntryPoint = new SandEntryPoint(500, 0);
        var cave = Cave.Parse(data, sandEntryPoint, !SuppressConsoleWriteLine, logWindow, infiniteWidth, heightExtension);
        var result = cave.EnableSand();

        WriteResult(part, result);

        if (expected != null)
        {
            result.Should().Be(expected);
        }
    }

    private abstract class Cell
    {
        public Vector Position { get; set; }
        public abstract char Sprite { get; }
        protected Cell(int x, int y)
        {
            Position = new Vector(x, y);
        }
    }

    private class Air : Cell
    {
        public override char Sprite => '.';
        public Air(int x, int y) : base(x, y)
        { }
    }

    private class Sand : Cell
    {
        public override char Sprite => 'o';
        public Sand(int x, int y) : base(x, y)
        { }
    }

    private class SandEntryPoint : Cell
    {
        public override char Sprite => '+';
        public SandEntryPoint(int x, int y) : base(x, y)
        { }

        public Sand Spawn()
            => new(Position.X, Position.Y);
    }

    private class Rock : Cell, IEquatable<Rock>
    {
        public override char Sprite => '#';
        public Rock(int x, int y) : base(x, y)
        { }

        public bool Equals(Rock other)
            => other != null && other.Position.Equals(Position);

        public override bool Equals(object obj)
            => Equals(obj as Rock);

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Position);
            return hashCode.ToHashCode();
        }
    }

    private class Cave
    {
        public SandEntryPoint SandEntryPoint { get; }
        public Rectangle LogWindow { get; }
        public Dictionary<(int, int), Cell> Cells { get; }
        public int Width { get; }
        public int Height { get; }
        public bool InfiniteWidth { get; }
        public static readonly Vector[] GravitationalPullOrder = new Vector[] { Vector.South, Vector.SouthWest, Vector.SouthEast };
        private readonly bool _loggingEnabled;

        public Cave(Dictionary<(int, int), Cell> cells, int width, int height, SandEntryPoint sandEntryPoint, bool enableLogging, Rectangle logWindow, bool infiniteWidth = false)
        {
            Cells = cells;
            SandEntryPoint = sandEntryPoint;
            LogWindow = logWindow;
            Width = width;
            Height = height;
            InfiniteWidth = infiniteWidth;
            _loggingEnabled = enableLogging;
        }

        public int EnableSand()
        {
            Draw();
            var enabled = true;
            var sandCount = 0;
            while (enabled)
            {
                var sand = SpawnSand();
                Draw();

                var nextPosition = ApplyGravity(sand);

                // If the sand has spanned and cannot move in any direction, 
                // we have reached the limit (i.e. part 2 scenario)
                if (nextPosition == null)
                {
                    sandCount++;
                    break;
                }

                while (nextPosition != null)
                {
                    try
                    {
                        nextPosition = ApplyGravity(sand);

                        if (nextPosition == null) continue;

                        MoveCell(sand, nextPosition);
                        Draw();
                    }
                    catch (IndexOutOfRangeException)
                    {
                        // If we've reached the edge of the boundary, this position is no good. 
                        // We can continue checking for other positions, but if none are found, 
                        // we've reached the limit, i.e. the part 1 scenario
                        nextPosition = null;
                        enabled = false;
                        AssignCell(new Air(sand.Position.X, sand.Position.Y));
                    }
                }

                if (!enabled) break;

                sandCount++;
            }

            return sandCount;
        }

        private Sand SpawnSand()
        {
            var sand = SandEntryPoint.Spawn();
            if (sand.Position == SandEntryPoint.Position) return null;
            AssignCell(sand);
            return sand;
        }

        private void MoveCell(Cell cell, Vector position)
        {
            AssignCell(new Air(cell.Position.X, cell.Position.Y));
            cell.Position = position;
            AssignCell(cell);
        }

        private void AssignCell(Cell cell)
        {
            Cells[(cell.Position.X, cell.Position.Y)] = cell;
        }

        private Vector ApplyGravity(Sand sand)
        {
            foreach (var direction in GravitationalPullOrder)
            {
                var position = sand.Position.Move(direction);

                bool horizontal = position.X != 0;

                if (!PositionInBounds(position))
                {
                    // If the out-of-bounds position is moving along the horizontal axis but infinite width is enabled, 
                    // we dont want o treat this as out of bounds. Instead, we just want to treat this as not a candidate position, 
                    // moving on to the next one (if there are any).
                    if (horizontal && InfiniteWidth) continue;

                    throw new IndexOutOfRangeException();
                }

                if (PositionAvailable(position))
                {
                    return position;
                }
            }
            return null;
        }

        public bool PositionAvailable(Vector position)
        {
            if (InfiniteWidth)
            {
                Cells.TryGetValue((position.X, position.Y), out var cell);
                return (cell == null && PositionInBounds(position)) || cell is Air;
            }

            return Cells[(position.X, position.Y)] is Air;
        }

        private bool PositionInBounds(Vector position)
        {
            return InfiniteWidth ? position.Y < Height : position.X < Width && position.Y < Height;
        }

        public static Cave Parse(string[] input, SandEntryPoint sandEntryPoint, bool enableLogging, Rectangle logWindow, bool infiniteWidth = false, int hightExtension = 0)
        {
            int maxX = sandEntryPoint.Position.X, maxY = sandEntryPoint.Position.Y;

            var rocks = new HashSet<Rock>();
            foreach (var row in input)
            {
                var corners = row.Split(" -> ").Select(position =>
                {
                    var parts = position.Split(",");
                    var x = int.Parse(parts.First());
                    var y = int.Parse(parts.Last());

                    maxX = Math.Max(x, maxX);
                    maxY = Math.Max(y, maxY);
                    return new Vector(x, y);
                });

                var structure = new List<List<Rock>>();
                for (var i = 0; i < corners.Count() - 1; i++)
                {
                    var line = new Line(corners.ElementAt(i), corners.ElementAt(i + 1));
                    line.GetPoints().ForEach(point => rocks.Add(new Rock(point.X, point.Y)));
                    Console.WriteLine($"Rock from {line.From} to {line.To}");
                }
            }

            return BuildCave(maxX + 1, maxY + 1 + hightExtension, rocks, sandEntryPoint, enableLogging, logWindow, infiniteWidth);
        }

        public void Draw()
        {
            if (_loggingEnabled) Console.WriteLine(ToString());
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            for (int y = LogWindow.From.Y; y <= LogWindow.To.Y; y++)
            {
                for (int x = LogWindow.From.X; x <= LogWindow.To.X; x++)
                {
                    if (Cells.TryGetValue((x, y), out var cell))
                    {
                        sb.Append(cell.Sprite);
                    }
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static Cave BuildCave(int maxX, int maxY, IEnumerable<Rock> rocks, SandEntryPoint sandEntryPoint, bool enableLogging, Rectangle logWindow, bool infiniteWidth)
        {
            // The max values are inclusive indexes, so we need to +1 when specifying the array lengths.
            Dictionary<(int, int), Cell> cells = new();

            for (int x = 0; x <= maxX; x++)
            {
                for (int y = 0; y <= maxY; y++)
                {
                    var rock = rocks.FirstOrDefault(r => r.Position.X == x && r.Position.Y == y);
                    cells[(x, y)] = (Cell)rock ?? new Air(x, y);
                }
            }

            cells[(sandEntryPoint.Position.X, sandEntryPoint.Position.Y)] = sandEntryPoint;

            return new Cave(cells, maxX, maxY, sandEntryPoint, enableLogging, logWindow, infiniteWidth);
        }
    }
}
