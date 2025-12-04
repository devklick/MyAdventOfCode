using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using MyAdventOfCode.Common;

using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2025.D01;

public class Solution : TestBase
{
    public Solution(ITestOutputHelper output) : base(output)
    { }

    [Fact]
    public override async Task Part1_Example()
        => await Invoke(Part.One, DataType.Example, dial => dial.GetStopCount(0), 3);

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke(Part.One, DataType.Actual, dial => dial.GetStopCount(0), 1089);

    [Fact]
    public override async Task Part2_Example()
        => await Invoke(Part.Two, DataType.Example, dial => dial.GetIncrementCount(0), 6);

    [Fact]
    public override async Task Part2_Actual()
        => await Invoke(Part.Two, DataType.Actual, dial => dial.GetIncrementCount(0), 6530);

    private async Task Invoke(Part part, DataType dataType, Func<RotaryLock, int> getResult, int? expected = null)
    {
        var data = await GetData(dataType, part);
        var instructions = RotationInstructions.Parse(data);
        var rotaryLock = new RotaryLock(0, 99, 50);

        rotaryLock.RotateMany(instructions);
        var result = getResult(rotaryLock);

        WriteResult(part, result);

        if (expected != null)
        {
            result.Should().Be(expected, part.ToString(), dataType.ToString());
        }
    }


    private class RotationInstruction
    {
        public Direction Direction { get; private set; }
        public int NumberOfClicks { get; private set; }

        public static RotationInstruction Parse(string raw) => new()
        {
            Direction = DirectionParser.Parse(raw.First()),
            NumberOfClicks = int.Parse(new string(raw.Skip(1).ToArray()))
        };
    }

    private class RotationInstructions : List<RotationInstruction>
    {
        public RotationInstructions(IEnumerable<RotationInstruction> rotationInstructions)
            : base(rotationInstructions) { }

        public static RotationInstructions Parse(IEnumerable<string> raw)
            => new(raw.Select(RotationInstruction.Parse));
    }
    private class RotaryLock
    {
        private Dictionary<int, int> StopCounts { get; } = new Dictionary<int, int>();
        private Dictionary<int, int> IncrementCounts { get; } = new Dictionary<int, int>();

        private int Min { get; }
        private int Max { get; }
        private int Current { get; set; }

        public RotaryLock(int min, int max, int start)
        {
            Min = min;
            Max = max;
            Current = start;
            IncrementCounts.Add(Current, 1);
            StopCounts.Add(Current, 1);
        }

        public int GetIncrementCount(int position)
            => IncrementCounts.GetValueOrDefault(position, 0);

        public int GetStopCount(int position)
            => StopCounts.GetValueOrDefault(position, 0);

        public void RotateMany(RotationInstructions instructions)
        {
            foreach (var instruction in instructions)
            {
                Rotate(instruction);
            }
        }

        /// <summary>
        /// Brute force each rotation. 
        /// 
        /// Far from optimal, but I wanted to keep track of each number we hit
        /// and how many times we hit it.
        /// </summary>
        private void Rotate(RotationInstruction instruction)
        {
            for (var i = 0; i < instruction.NumberOfClicks; i++)
            {
                Current = Next(instruction.Direction);
                IncrementCounts[Current] = IncrementCounts.GetValueOrDefault(Current, 0) + 1;
            }
            StopCounts[Current] = StopCounts.GetValueOrDefault(Current, 0) + 1;
        }

        private int Next(Direction direction)
        {
            var next = Current + (int)direction;
            if (next > Max) return Min;
            if (next < Min) return Max;
            return next;
        }
    }
}