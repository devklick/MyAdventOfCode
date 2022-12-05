using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using MyAdventOfCode.Common;
using MyAdventOfCode.Extensions;

using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2022.D05;

public class Solution : TestBase
{
    public Solution(ITestOutputHelper output) : base(output)
    { }

    [Fact]
    public override async Task Part1_VerifyExample()
        => await Invoke(Part.One, DataType.Example, "CMZ");

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke(Part.One, DataType.Actual, "PTWLTDSJV");

    [Fact]
    public override async Task Part2_VerifyExample()
        => await Invoke(Part.Two, DataType.Example, "MCD");

    [Fact]
    public override async Task Part2_Actual()
        => await Invoke(Part.Two, DataType.Actual, "WZMFVGGZP");

    private async Task Invoke(Part part, DataType dataType, string expected = null)
    {
        var data = await GetData(dataType);
        var parser = new Parser(data.ToList());
        var createStacks = parser.GetCrateStacks();
        var instructions = parser.GetInstructions(part);

        var cargoBay = new CargoBay(createStacks);
        cargoBay.RearrangeCrates(instructions);

        var result = cargoBay.GetTopCratesAsString();

        WriteResult(part, result);

        if (expected != null)
        {
            result.Should().Be(expected, part.ToString());
        }
    }

    private class CargoBay
    {
        private readonly Dictionary<int, List<char>> _crateStacks;

        public CargoBay(Dictionary<int, List<char>> crateStacks)
        {
            _crateStacks = crateStacks;
        }

        public void RearrangeCrates(IEnumerable<RearrangeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                var crates = _crateStacks[instruction.From].GetRange(_crateStacks[instruction.From].Count - instruction.NumberOfItems, instruction.NumberOfItems);
                _crateStacks[instruction.From].RemoveRange(_crateStacks[instruction.From].Count - instruction.NumberOfItems, instruction.NumberOfItems);
                if (!instruction.MaintainOrder) crates.Reverse();
                _crateStacks[instruction.To].AddRange(crates);
            }
        }

        public string GetTopCratesAsString() => new(_crateStacks.Values.Select(stack => stack.Last()).ToArray());
    }

    private class Parser
    {
        private readonly List<string> _rawData;
        private readonly int _splitAt;
        public Parser(List<string> rawData)
        {
            _rawData = rawData;
            // Get the first line that contains only white space - this separates the cargo data (above) and the instruction dat (below)
            _splitAt = _rawData.FindIndex(string.IsNullOrWhiteSpace);
        }

        public Dictionary<int, List<char>> GetCrateStacks()
        {
            // Get he section of data that tells us about the cargo, and flip it to make it easier to work with
            var cargoData = _rawData.GetRange(0, _splitAt);
            cargoData.Reverse();

            // Get the "stack names", i.e. the numbers at the bottom (now top) of the cargo data
            var stackNames = cargoData.First().Split().Where(c => !string.IsNullOrWhiteSpace(c)).Select(int.Parse).ToList();

            var stacks = stackNames
                .Select(stack => new KeyValuePair<int, List<char>>(stack, new List<char>()))
                .ToDictionary(x => x.Key, x => x.Value);

            foreach (var line in cargoData.Skip(1))
            {
                foreach (var (i, part) in line.SplitInParts(4))
                {
                    // If the string contains at least 2 characters, and the char at the second position is not a space, 
                    // that's the character for crate. Otherwise ignore it.
                    if (part.Length >= 2 && part[1] != ' ')
                    {
                        var partNo = i + 1;
                        if (stacks[partNo] == null) stacks[partNo] = new List<char>();
                        stacks[partNo].Add(part[1]);
                    }
                }
            }

            return stacks;
        }

        public IEnumerable<RearrangeInstruction> GetInstructions(Part part)
            => _rawData.GetRange(_splitAt, _rawData.Count - _splitAt)
            .Where(instruction => !string.IsNullOrWhiteSpace(instruction))
            .Select(instruction =>
            {
                var parts = instruction.Split();
                int total = int.Parse(parts[1]), from = int.Parse(parts[3]), to = int.Parse(parts[5]);
                return new RearrangeInstruction(total, from, to, part == Part.Two);
            });

    }

    private class RearrangeInstruction
    {
        public int NumberOfItems { get; }
        public int From { get; }
        public int To { get; }
        public bool MaintainOrder { get; set; }

        public RearrangeInstruction(int numberOfItems, int from, int to, bool maintainOrder)
        {
            NumberOfItems = numberOfItems;
            From = from;
            To = to;
            MaintainOrder = maintainOrder;
        }
    }
}