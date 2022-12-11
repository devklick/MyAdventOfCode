using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using MyAdventOfCode.Common;

using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2022.D09;

public class Solution : TestBase
{
    public Solution(ITestOutputHelper output) : base(output)
    { }

    [Fact]
    public override async Task Part1_Example()
        => await Invoke_Part1(DataType.Example, 13);

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke_Part1(DataType.Actual, 5735);

    [Fact]
    public override async Task Part2_Example()
        => await Invoke_Part2(DataType.Example, 36);

    [Fact]
    public override async Task Part2_Actual()
        => await Invoke_Part2(DataType.Actual, 2478);

    private async Task Invoke_Part1(DataType dataType, int? expected = null)
        => await Invoke(
            Part.One,
            dataType,
            2,
            expected);

    private async Task Invoke_Part2(DataType dataType, int? expected = null)
        => await Invoke(
            Part.Two,
            dataType,
            10,
            expected);

    private async Task Invoke(Part part, DataType dataType, int ropeLength, int? expected = null)
    {
        var data = await GetData(dataType, dataType == DataType.Example ? part : null);
        var tugs = Tug.ParseMany(data);
        var rope = new Rope(ropeLength);
        var tugger = new Tugger(rope);
        tugger.Tug(tugs);
        var result = tugger.Rope.Tail.History.Distinct().Count();

        WriteResult(part, result);

        if (expected.HasValue)
        {
            result.Should().Be(expected);
        }
    }

    private class Knot
    {
        private RCVector _position;
        public int KnotNumber { get; }
        public bool IsHead => KnotNumber == 0;
        public List<RCVector> History { get; } = new();
        public RCVector Position
        {
            get => _position;
            set
            {
                History.Add(value);
                _position = value;
            }
        }
        public Knot(int knotNumber)
        {
            Position = RCVector.Default;
            KnotNumber = knotNumber;
        }
    }

    private class Rope : List<Knot>
    {
        public Knot Head => this.First();
        public Knot Tail => this.Last();
        public Rope(int length)
        {
            for (var no = 0; no < length; no++)
            {
                Add(new Knot(no));
            }
        }
    }

    private class Tugger
    {
        public Rope Rope { get; }

        public Tugger(Rope rope)
        {
            Rope = rope;
        }

        public void Tug(IEnumerable<Tug> tugs)
        {
            foreach (var tug in tugs)
            {
                Console.WriteLine(tug);
                for (var time = 0; time < tug.Times; time++)
                {
                    foreach (var knot in Rope)
                    {
                        if (knot.IsHead) MoveHead(tug.Direction);
                        else MoveTail(knot, Rope.First(k => k.KnotNumber == knot.KnotNumber - 1));
                    }
                }
            }
        }

        private void MoveHead(RCVector direction)
        {
            var position = Rope.Head.Position.Move(direction);
            Console.WriteLine($"Head: Moving to {position}");
            Rope.Head.Position = position;
        }

        private void MoveTail(Knot knot, Knot previousKnot)
        {
            if (knot.Position == previousKnot.Position || knot.Position.IsNextTo(previousKnot.Position))
            {
                Console.WriteLine("Tail: Not moving");
                return;
            }

            foreach (var cardinal in RCVector.Cardinals)
            {
                if (knot.Position.IsCardinallyAlignedWith(previousKnot.Position)
                    && knot.Position.DistanceFrom(previousKnot.Position, cardinal) == 2)
                {
                    var position = knot.Position.Move(cardinal);
                    Console.WriteLine($"Tail: Moving to {position}");
                    knot.Position = position;
                    return;
                }
            }

            foreach (var ordinal in RCVector.Ordinals)
            {
                var candidate = knot.Position.Move(ordinal);

                if (candidate.IsNextTo(previousKnot.Position))
                {
                    Console.WriteLine($"Tail: Moving to {candidate}");
                    knot.Position = candidate;
                    return;
                }
            }

            throw new NotImplementedException("I fucked up");
        }
    }

    private class Tug
    {
        public RCVector Direction { get; }
        public int Times { get; }

        public Tug(RCVector direction, int times)
        {
            Direction = direction;
            Times = times;
        }

        public static IEnumerable<Tug> ParseMany(string[] input)
            => input.Select(Parse);

        public static Tug Parse(string input)
        {
            var parts = input.Split();

            if (parts.Length != 2)
                throw new InvalidOperationException("Expected input to contain two values separated by a space");

            var direction = RCVector.Parse(parts.First());
            var times = int.Parse(parts.Last());
            return new Tug(direction, times);
        }

        public override string ToString()
            => $"== {GetDirection()} {Times} ==";

        private string GetDirection() => Direction switch
        {
            NorthRCVector => "U",
            EastRCVector => "R",
            SouthRCVector => "D",
            WestRCVector => "L",
            _ => throw new NotImplementedException("Direction not yet supported"),
        };
    }
}