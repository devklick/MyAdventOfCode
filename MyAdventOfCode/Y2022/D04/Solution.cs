using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using MyAdventOfCode.Common;

using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2022.D04;

public class Solution : TestBase
{
    public Solution(ITestOutputHelper output) : base(output)
    { }

    [Fact]
    public override async Task Part1_Example()
        => await Invoke(Part.One, DataType.Example, (sut) => sut.GetTotalAllocationDuplication(), 2);

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke(Part.One, DataType.Actual, (sut) => sut.GetTotalAllocationDuplication(), 542); // 542

    [Fact]
    public override async Task Part2_Example()
        => await Invoke(Part.Two, DataType.Example, (sut) => sut.GetTotalAllocationOverlaps(), 4);

    [Fact]
    public override async Task Part2_Actual()
        => await Invoke(Part.Two, DataType.Actual, (sut) => sut.GetTotalAllocationOverlaps(), 900); // 900

    private async Task Invoke(Part part, DataType dataType, Func<CleaningRota, int> sut, int? expected = null)
    {
        var data = await GetData(dataType);
        var rota = new CleaningRota(data);
        var result = sut?.Invoke(rota);

        WriteResult(part, result);

        if (expected != null)
        {
            result.Should().Be(expected, part.ToString());
        }
    }

    private class SectionAllocation : List<int>
    {
        public bool Inclusive { get; }
        public SectionAllocation(int from, int to, bool inclusive = true)
        {
            Inclusive = inclusive;
            AddRange(Inclusive
                ? Enumerable.Range(from, to - from + 1)
                : Enumerable.Range(from + 1, to - from - 1));
        }

        public bool Contains(SectionAllocation others)
            => others.All(Contains);
    }

    private class Cleaner
    {
        public int Id { get; }
        public SectionAllocation AllocatedSections { get; }

        public Cleaner(int id, string cleanerAllocationTemplate)
        {
            Id = id;
            var (from, to) = ParseTemplate(cleanerAllocationTemplate);
            AllocatedSections = new SectionAllocation(from, to);
        }

        private static (int From, int To) ParseTemplate(string template)
        {
            var split = template.Split('-');
            if (split.Length != 2) throw new ArgumentException("Expected input to contain a hyphen");
            if (!int.TryParse(split.First(), out var from)) throw new Exception("Expected left side of hyphen to be a number");
            if (!int.TryParse(split.Last(), out var to)) throw new Exception("Expected right side of hyphen to be a number");
            return (from, to);
        }
    }

    private class CleaningCrew : List<Cleaner>
    {
        public CleaningCrew(string crewAllocationTemplate)
        {
            int cleanerId = 1;
            foreach (var cleanerAllocationTemplate in crewAllocationTemplate.Split(','))
            {
                Add(new Cleaner(cleanerId, cleanerAllocationTemplate));
                cleanerId++;
            }
        }

        public bool HasAllocationDuplication()
            => CompareAllocations((a, b) => a.Contains(b));

        public bool HasAllocationOverlap()
            => CompareAllocations((a, b) => a.Any(x => b.Any(y => x == y)));

        private bool CompareAllocations(Func<SectionAllocation, SectionAllocation, bool> compare)
        {
            foreach (var cleaner in this)
            {
                if (this.Any(c => c.Id != cleaner.Id && compare.Invoke(c.AllocatedSections, cleaner.AllocatedSections)))
                {
                    return true;
                }
            }
            return false;
        }
    }

    private class CleaningRota : List<CleaningCrew>
    {
        public CleaningRota(string[] data)
        {
            foreach (var line in data)
            {
                Add(new CleaningCrew(line));
            }
        }

        public int GetTotalAllocationDuplication()
            => this.Count(s => s.HasAllocationDuplication());

        public int GetTotalAllocationOverlaps()
            => this.Count(s => s.HasAllocationOverlap());
    }
}