using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using MyAdventOfCode.Common;

using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2022.D01;

public class Solution : TestBase
{
    protected override int DayNo => 1;
    protected override int YearNo => 2022;
    public Solution(ITestOutputHelper output) : base(output)
    { }

    [Fact]
    public override async Task Part1_VerifyExample()
        => await Invoke(Part.One, DataType.Example, 1, 24000);

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke(Part.One, DataType.Actual, 1, 70613); // 70613

    [Fact]
    public override async Task Part2_VerifyExample()
        => await Invoke(Part.Two, DataType.Example, 3, 45000);

    [Fact]
    public override async Task Part2_Actual()
        => await Invoke(Part.Two, DataType.Actual, 3, 205805);

    private async Task Invoke(Part part, DataType dataType, int stashesToTake, int? expected = null)
    {
        var data = await GetData(dataType);
        var inventory = new CalorificInventory(data);

        var result = inventory.GetTotalOfHighestCalorificStashes(stashesToTake);

        WriteResult(part, result);

        if (expected != null)
        {
            result.Should().Be(expected, part.ToString());
        }
    }

    private class CalorificInventory
    {
        private readonly List<ElfStash> _elfStashes = new();
        public CalorificInventory(string[] data)
        {
            var snacks = new List<int>();
            var elfNo = 0;

            foreach (var line in data)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    _elfStashes.Add(new ElfStash(elfNo, snacks));
                    snacks = new List<int>();
                    elfNo++;
                    continue;
                }

                snacks.Add(int.Parse(line.Trim()));
            }
            if (snacks.Any())
            {
                _elfStashes.Add(new ElfStash(elfNo, snacks));
            }
            _elfStashes = _elfStashes.OrderByDescending(stash => stash.TotalCalories).ToList();
        }

        public int GetTotalOfHighestCalorificStashes(int numberOfStashes = 1)
             => _elfStashes.Take(numberOfStashes).Sum(stash => stash.TotalCalories);
    }

    private class ElfStash
    {
        public int ElfNo { get; set; }
        public List<int> Snacks { get; set; }
        public int TotalCalories => Snacks.Sum();

        public ElfStash(int elfNo, List<int> snacks)
        {
            ElfNo = elfNo;
            Snacks = snacks;
        }
    }
}