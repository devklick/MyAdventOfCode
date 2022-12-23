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
    public Solution(ITestOutputHelper output) : base(output, suppressConsoleWriteLine: false)
    { }

    [Fact]
    public override async Task Part1_Example()
        => await Invoke(Part.One, DataType.Example, 24);

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke(Part.One, DataType.Actual, 828);

    [Fact]
    public override async Task Part2_Example()
        => await Invoke_Part2(DataType.Example);

    [Fact]
    public override async Task Part2_Actual()
        => await Invoke_Part2(DataType.Actual);

    private async Task Invoke_Part2(DataType dataType, int? expected = null)
    {

    }

    private async Task Invoke(Part part, DataType dataType, int? expected = null)
    {
        var data = await GetData(dataType);
        var logWindow = new Rectangle(494, 0, 503, 9);
        var sandEntryPoint = new SandEntryPoint(500, 0);
        var cave = Cave.Parse(data, sandEntryPoint, logWindow);
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
        {
            var position = Position.Move(Vector.South);
            return new(position.X, position.Y);
        }
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
        public Cell[,] Cells { get; }
        public int Width => Cells.GetLength(0);
        public int Height => Cells.GetLength(1);
        public readonly Vector[] GravityOrder = new Vector[] { Vector.South, Vector.SouthWest, Vector.SouthEast };

        public Cave(Cell[,] cells, SandEntryPoint sandEntryPoint, Rectangle logWindow)
        {
            Cells = cells;
            SandEntryPoint = sandEntryPoint;
            LogWindow = logWindow;
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

                if (nextPosition == null) break;

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
            Cells[cell.Position.X, cell.Position.Y] = cell;
        }

        private Vector ApplyGravity(Sand sand)
        {
            foreach (var direction in GravityOrder)
            {
                var position = sand.Position.Move(direction);

                if (!PositionInBounds(position))
                {
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
            => Cells[position.X, position.Y] is Air;

        private bool PositionInBounds(Vector position)
            => position.X < Cells.GetLength(0) && position.Y < Cells.GetLength(1);

        public static Cave Parse(string[] input, SandEntryPoint sandEntryPoint, Rectangle logWindow)
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

            return BuildCave(maxX + 1, maxY + 1, rocks, sandEntryPoint, logWindow);
        }

        public void Draw() => Console.WriteLine(ToString());

        public override string ToString()
        {
            var sb = new StringBuilder();

            for (int y = LogWindow.From.Y; y <= LogWindow.To.Y; y++)
            {
                for (int x = LogWindow.From.X; x <= LogWindow.To.X; x++)
                {
                    sb.Append(Cells[x, y].Sprite);
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static Cave BuildCave(int maxX, int maxY, IEnumerable<Rock> rocks, SandEntryPoint sandEntryPoint, Rectangle logWindow)
        {
            // The max values are inclusive indexes, so we need to +1 when specifying the array lengths.
            Cell[,] cells = new Cell[maxX + 1, maxY + 1];

            for (int x = 0; x <= maxX; x++)
            {
                for (int y = 0; y <= maxY; y++)
                {
                    var rock = rocks.FirstOrDefault(r => r.Position.X == x && r.Position.Y == y);
                    cells[x, y] = (Cell)rock ?? new Air(x, y);
                }
            }

            cells[sandEntryPoint.Position.X, sandEntryPoint.Position.Y] = sandEntryPoint;

            return new Cave(cells, sandEntryPoint, logWindow);
        }
    }
}
