using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using MyAdventOfCode.Common;
using MyAdventOfCode.Extensions;

using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2022.D03;

public class Solution : TestBase
{
    protected override int DayNo => 3;
    protected override int YearNo => 2022;

    public Solution(ITestOutputHelper output) : base(output)
    { }

    [Fact]
    public override async Task Part1_VerifyExample()
        => await Invoke(Part.One, DataType.Example, (loader) => loader.GetTotalCommonItemsPriority(), 157);

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke(Part.One, DataType.Actual, (loader) => loader.GetTotalCommonItemsPriority(), 8233); // 8233

    [Fact]
    public override async Task Part2_VerifyExample()
        => await Invoke(Part.Two, DataType.Example, (loader) => loader.GetGroupStickerPriority(3), 70);

    [Fact]
    public override async Task Part2_Actual()
        => await Invoke(Part.Two, DataType.Actual, (loader) => loader.GetGroupStickerPriority(3)); // 13889

    private delegate int InvokeRucksackLoaderDelegate(RucksackLoader loader);
    private async Task Invoke(Part part, DataType dataType, InvokeRucksackLoaderDelegate loaderCallback, int? expected = null)
    {
        var data = await GetData(dataType);
        var loader = new RucksackLoader(data);
        var result = loaderCallback?.Invoke(loader);

        WriteResult(part, result);

        if (expected != null)
        {
            result.Should().Be(expected, part.ToString());
        }
    }

    private class RucksackLoader
    {
        private readonly List<Rucksack> _rucksacks = new();
        public RucksackLoader(string[] data)
        {
            foreach (var line in data)
            {
                _rucksacks.Add(new Rucksack(line.ToList(), 2));
            }
        }

        public int GetTotalCommonItemsPriority() => _rucksacks.Sum(s => s.GetTotalCommonItemsPriority());

        public int GetGroupStickerPriority(int groupSize)
        {
            if (_rucksacks.Count % groupSize != 0)
            {
                throw new ArgumentException($"Unable to evenly split {_rucksacks.Count} rucksacks into groups of {groupSize}");
            }
            int total = 0;
            for (var i = 0; i < _rucksacks.Count; i += groupSize)
            {
                var groupRucksacks = _rucksacks.GetRange(i, Math.Min(groupSize, _rucksacks.Count - 1));
                var common = groupRucksacks.Select(b => b.GetAllItems()).IntersectAll();
                var priority = common.Sum(c => Item.ValueMap[c]);
                total += priority;
            }

            return total;
        }
    }

    private class Rucksack
    {
        private readonly List<Compartment> _compartments = new();

        public Rucksack(List<char> items, int numberOfCompartments)
        {
            if (items.Count % numberOfCompartments != 0)
            {
                throw new ArgumentException($"Unable to evenly split {items.Count} items into {numberOfCompartments} compartments");
            }
            var compartmentSize = items.Count / numberOfCompartments;
            for (var i = 0; i < items.Count; i += compartmentSize)
            {
                _compartments.Add(new Compartment(items.GetRange(i, Math.Min(compartmentSize, items.Count - 1))));
            }
        }

        public List<char> GetCommonItems()
        {
            var set = new HashSet<char>(_compartments.First().Items);
            foreach (var compartment in _compartments.Skip(1))
            {
                set.IntersectWith(compartment.Items);
            }
            return set.ToList();
        }

        public int GetTotalCommonItemsPriority() => GetCommonItems().Sum(c => Item.ValueMap[c]);

        public List<char> GetAllItems() => _compartments.SelectMany(c => c.Items).ToList();
    }

    private class Compartment
    {
        public List<char> Items = new();

        public Compartment(List<char> items)
        {
            Items = items;
        }
    }

    public struct Item : IEquatable<char>
    {
        public static readonly Dictionary<char, int> ValueMap = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
            .Select((character, index) => (character, index))
            .ToDictionary(x => x.character, x => x.index + 1);

        private readonly char _char;

        public Item(char c)
        {
            _char = c;
        }

        public bool Equals(char other)
            => other == _char;
    }
}