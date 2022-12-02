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
        => await PartN_VerifyExample(Part.One, 1, 24000);

    [Fact]
    public override async Task Part1_Actual()
        => await PartN_Actual(Part.One, 1); // 70613

    [Fact]
    public override async Task Part2_VerifyExample()
        => await PartN_VerifyExample(Part.Two, 3, 45000);

    [Fact]
    public override async Task Part2_Actual()
        => await PartN_Actual(Part.Two, 3); // 205805

    private async Task PartN_VerifyExample(Part part, int stashesToTake, int expected)
    {
        var data = await GetExampleData();
        var inventory = new CalorificInventory(data);

        var result = inventory.GetTotalOfHighestCalorificStashes(stashesToTake);

        result.Should().Be(expected, part.ToString());
    }

    private async Task PartN_Actual(Part part, int stashesToTake)
    {
        var data = await GetData();
        var inventory = new CalorificInventory(data);

        var result = inventory.GetTotalOfHighestCalorificStashes(stashesToTake);

        WriteResult(part, result);
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