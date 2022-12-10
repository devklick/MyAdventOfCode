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
        => await Invoke_Part1(DataType.Actual);

    [Fact]
    public override async Task Part2_Example()
        => await Invoke_Part2(DataType.Example);


    [Fact]
    public override async Task Part2_Actual()
        => await Invoke_Part2(DataType.Actual);

    private async Task Invoke_Part1(DataType dataType, int? expected = null)
        => await Invoke(
            Part.Two,
            dataType,
            expected);

    private async Task Invoke_Part2(DataType dataType, int? expected = null)
        => await Invoke(
            Part.Two,
            dataType,
            expected);

    private async Task Invoke(Part part, DataType dataType, int? expected = null)
    {
        var data = await GetData(dataType);
        var tugs = Tug.ParseMany(data);
        var tugger = new Tugger();
        tugger.Tug(tugs);
        var result = tugger.TailPositionHistory.Distinct().Count();

        WriteResult(part, result);

        if (expected.HasValue)
        {
            result.Should().Be(expected);
        }
    }

    public class Tugger
    {
        private RCVector _head;
        private RCVector _tail;
        public List<RCVector> HeadPositionHistory { get; } = new();
        public List<RCVector> TailPositionHistory { get; } = new();
        public RCVector HeadPosition
        {
            get { return _head; }
            set
            {
                HeadPositionHistory.Add(value);
                _head = value;
            }
        }
        public RCVector TailPosition
        {
            get { return _tail; }
            set
            {
                TailPositionHistory.Add(value);
                _tail = value;
            }
        }

        public Tugger()
        {
            HeadPosition = RCVector.Default;
            TailPosition = RCVector.Default;
        }

        public void Tug(IEnumerable<Tug> tugs)
        {
            foreach (var tug in tugs)
            {
                Console.WriteLine(tug);
                for (var time = 0; time < tug.Times; time++)
                {
                    MoveHead(tug.Direction);
                    MoveTail();
                }
            }
        }

        private void MoveHead(RCVector direction)
        {
            var position = HeadPosition.Move(direction);
            Console.WriteLine($"Head: Moving to {position}");
            HeadPosition = position;
        }

        private void MoveTail()
        {
            if (TailPosition == HeadPosition || TailPosition.IsNextTo(HeadPosition))
            {
                Console.WriteLine("Tail: Not moving");
                return;
            }

            foreach (var cardinal in RCVector.Cardinals)
            {
                if (TailPosition.IsCardinallyAlignedWith(HeadPosition)
                    && TailPosition.DistanceFrom(HeadPosition, cardinal) == 2)
                {
                    var position = TailPosition.Move(cardinal);
                    Console.WriteLine($"Tail: Moving to {position}");
                    TailPosition = position;
                    return;
                }
            }

            foreach (var ordinal in RCVector.Ordinals)
            {
                var candidate = TailPosition.Move(ordinal);

                if (candidate.IsNextTo(HeadPosition))
                {
                    Console.WriteLine($"Tail: Moving to {candidate}");
                    TailPosition = candidate;
                    return;
                }
            }

            throw new NotImplementedException("I fucked up");
        }
    }

    public class Tug
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