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
    public Solution(ITestOutputHelper output) : base(output)
    { }

    [Fact]
    public override async Task Part1_VerifyExample()
        => await Invoke(Part.One, DataType.Example, (sut) => sut.GetTotalCommonItemsPriority(), 157);

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke(Part.One, DataType.Actual, (sut) => sut.GetTotalCommonItemsPriority(), 8233); // 8233

    [Fact]
    public override async Task Part2_VerifyExample()
        => await Invoke(Part.Two, DataType.Example, (sut) => sut.GetGroupStickerPriority(3), 70);

    [Fact]
    public override async Task Part2_Actual()
        => await Invoke(Part.Two, DataType.Actual, (sut) => sut.GetGroupStickerPriority(3), 2821); // 2821

    private async Task Invoke(Part part, DataType dataType, Func<RucksackLoader, int> sut, int? expected = null)
    {
        var data = await GetData(dataType);
        var loader = new RucksackLoader(data);
        var result = sut?.Invoke(loader);

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
                _rucksacks.Add(new Rucksack(line.Select(c => new Item(c)).ToList(), 2));
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
                var priority = common.Sum(item => item.Value);
                total += priority;
            }

            return total;
        }
    }

    private class Rucksack
    {
        private readonly List<Compartment> _compartments = new();

        public Rucksack(List<Item> items, int numberOfCompartments)
        {
            if (items.Count % numberOfCompartments != 0)
            {
                throw new ArgumentException($"Unable to evenly split {items.Count} items into {numberOfCompartments} compartments");
            }
            var compartmentSize = items.Count / numberOfCompartments;
            for (var i = 0; i < items.Count; i += compartmentSize)
            {
                var compartmentItems = items.GetRange(i, Math.Min(compartmentSize, items.Count - 1));
                var compartment = new Compartment(compartmentItems.ToList());
                _compartments.Add(compartment);
            }
        }

        public List<Item> GetCommonItems()
        {
            var set = new HashSet<Item>(_compartments.First().Items);
            foreach (var compartment in _compartments.Skip(1))
            {
                set.IntersectWith(compartment.Items);
            }
            return set.ToList();
        }

        public int GetTotalCommonItemsPriority() => GetCommonItems().Sum(item => item.Value);

        public List<Item> GetAllItems() => _compartments.SelectMany(c => c.Items).ToList();
    }

    private class Compartment
    {
        public List<Item> Items = new();

        public Compartment(List<Item> items)
        {
            Items = items;
        }
    }

    public struct Item : IEquatable<char>
    {
        public static readonly Dictionary<char, int> ValueMap = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
            .Select((character, index) => (character, index))
            .ToDictionary(x => x.character, x => x.index + 1);

        public readonly int Value;
        private readonly char _char;

        public Item(char c)
        {
            _char = c;
            Value = ValueMap[_char];
        }

        public bool Equals(char other)
            => other == _char;
    }
}