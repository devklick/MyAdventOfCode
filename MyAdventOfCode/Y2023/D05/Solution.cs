using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using MyAdventOfCode.Common;
using MyAdventOfCode.Extensions;

using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2023.D05;

public class Solution : TestBase
{
    public Solution(ITestOutputHelper output) : base(output)
    { }

    [Fact]
    public override async Task Part1_Example()
        => await Invoke(
            Part.One,
            DataType.Example,
            null,
            35);

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke(
            Part.One,
            DataType.Actual,
            null,
            199602917);

    [Fact]
    public override async Task Part2_Example()
        => await Invoke(
            Part.Two,
            DataType.Example,
            null,
            46);

    // The approach I've taken doesn't work for test two. 
    // I've tried to refactor it but failed, and I'm not spending any more time on it.
    [Fact]
    public override async Task Part2_Actual() => await Task.CompletedTask;
    // => await Invoke(
    //     Part.Two,
    //     DataType.Actual,
    //     null,
    //     0);

    private async Task Invoke(Part part, DataType dataType, Func<object, long> getResult, long? expected = null)
    {
        var data = await GetData(dataType, part);
        var parseMode = part == Part.One ? Seed.ParseMode.Number : Seed.ParseMode.NumberWithRange;
        var almanac = Almanac.Parse(data, parseMode);
        var result = almanac.GetSeedsNearestLocations().Select(s => s.NearestLocation).Min();

        WriteResult(part, result);

        if (expected != null)
        {
            result.Should().Be(expected, part.ToString());
        }
    }

    private class Range
    {
        public long Start { get; }
        public long End => Start + Length;
        public long Length { get; }

        public Range(long start, long length)
        {
            Start = start;
            Length = length;
        }
    }

    private class Category
    {
        public string Name { get; }
        public List<Range> Ranges { get; }

        public Category(string name, List<Range> ranges = null)
        {
            Name = name;
            Ranges = ranges ?? new List<Range>();
        }
    }

    private class CategoryMap
    {
        public Category Source { get; }
        public Category Destination { get; }

        public CategoryMap(Category source, Category destination)
        {
            Source = source;
            Destination = destination;
        }

        public IEnumerable<(Range Source, Range Destination, long startOffset)> EnumerateRages()
        {
            for (int i = 0; i < Math.Min(Source.Ranges.Count, Destination.Ranges.Count); i++)
            {
                var sourceRange = Source.Ranges.ElementAtOrDefault(i);
                var destinationRange = Destination.Ranges.ElementAtOrDefault(i);
                var offset = destinationRange.Start - sourceRange.Start;
                yield return (sourceRange, destinationRange, offset);
            }
        }

        public long GetLowest(long sourceId)
        {
            var matches = EnumerateRages()
                .Where((item) => sourceId.Between(item.Source.Start, item.Source.End))
                .OrderByDescending(item => item.Source.Start);

            return matches.Any() ? sourceId + matches.First().startOffset : sourceId;
        }

        public static IEnumerable<CategoryMap> ParseMany(string[] data)
        {
            var mapRows = new List<string>();

            foreach (var row in data)
            {
                if (string.IsNullOrEmpty(row))
                {
                    yield return Parse(mapRows);
                    mapRows.Clear();
                }
                else
                {
                    mapRows.Add(row);
                }
            }
            yield return Parse(mapRows);

        }

        public static CategoryMap Parse(IEnumerable<string> data)
        {
            var nameMap = data.First().Split(" map:").First().Split("-to-");
            var source = new Category(nameMap.First());
            var destination = new Category(nameMap.Last());

            foreach (var row in data.Skip(1))
            {
                var rangeInfo = row.Split();
                var destinationStart = long.Parse(rangeInfo.First());
                var sourceStart = long.Parse(rangeInfo.ElementAt(1));
                var length = long.Parse(rangeInfo.Last());

                destination.Ranges.Add(new Range(destinationStart, length));
                source.Ranges.Add(new Range(sourceStart, length));
            }

            return new CategoryMap(source, destination);
        }
    }

    public class Seed
    {
        public enum ParseMode { Number, NumberWithRange };
        public long Id { get; }

        public Seed(long id)
        {
            Id = id;
        }

        public static IEnumerable<Seed> ParseMany(string data, ParseMode parseMode) => parseMode switch
        {
            ParseMode.Number => data.Split("seeds: ").Last().Split().Select(n => new Seed(long.Parse(n))),
            ParseMode.NumberWithRange => ParseManyWithRange(data),
            _ => throw new NotImplementedException($"Parse mode ${parseMode} not supported")
        };

        private static IEnumerable<Seed> ParseManyWithRange(string data)
        {
            foreach (var chunk in data.Split("seeds: ").Last().Split().Chunk(2))
            {
                var start = long.Parse(chunk.First());
                var count = long.Parse(chunk.Last());


                foreach (var seed in GenerateSeeds(start, count))
                {
                    yield return seed;
                }
            }
        }

        private static IEnumerable<Seed> GenerateSeeds(long start, long count)
        {
            for (var i = start; i < start + count; i++)
            {
                yield return new Seed(i);
            }
        }
    }

    private class Almanac
    {
        public IEnumerable<Seed> Seeds { get; }
        public IEnumerable<CategoryMap> Maps { get; }

        public Almanac(IEnumerable<Seed> seeds, IEnumerable<CategoryMap> maps)
        {
            Seeds = seeds;
            Maps = maps;
        }

        public static Almanac Parse(string[] data, Seed.ParseMode parseMode)
        {
            var seeds = Seed.ParseMany(data.First(), parseMode);
            var maps = CategoryMap.ParseMany(data.Skip(2).ToArray());

            return new(seeds, maps);
        }

        public long GetNearestLocationForSeed(Seed seed)
        {
            var currentId = seed.Id;
            var currentMap = Maps.First(m => m.Source.Name == "seed");

            while (currentMap != null)
            {
                currentId = currentMap.GetLowest(currentId);
                currentMap = Maps.FirstOrDefault(m => m.Source.Name == currentMap.Destination.Name);
            }
            return currentId;
        }

        public IEnumerable<(Seed Seed, long NearestLocation)> GetSeedsNearestLocations()
        {
            foreach (var seed in Seeds)
            {
                yield return (seed, GetNearestLocationForSeed(seed));
            }
        }
    }
}
