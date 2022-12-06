using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using MyAdventOfCode.Common;
using MyAdventOfCode.Extensions;

using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2022.D06;

public class Solution : TestBase
{
    public Solution(ITestOutputHelper output) : base(output)
    { }

    private class TestData
    {
        public string Input { get; set; }
        public int Expected { get; set; }

        public TestData(string input, int expected)
        {
            Input = input;
            Expected = expected;
        }
    }

    [Fact]
    public override async Task Part1_Example()
        => InvokeExample(Part.One, 4, new List<TestData>
        {
            new("mjqjpqmgbljsphdztnvjfqwrcgsmlb", 7),
            new("bvwbjplbgvbhsrlpgdmjqwftvncz",5),
            new("nppdvjthqldpwncqszvftbrmjlhg", 6),
            new("nznrnfrfntjfmvfwmzdfjlvtqnbhcprsg", 10),
            new("zcfzfwzzqfrljwzlrfnpqdbhtmscgvjw", 11),
        });

    [Fact]
    public override async Task Part1_Actual()
        => await InvokeActual(Part.One, 4, 1155);

    [Fact]
    public override async Task Part2_Example()
        => InvokeExample(Part.Two, 14, new List<TestData>
        {
            new("mjqjpqmgbljsphdztnvjfqwrcgsmlb", 19),
            new("bvwbjplbgvbhsrlpgdmjqwftvncz",23),
            new("nppdvjthqldpwncqszvftbrmjlhg", 23),
            new("nznrnfrfntjfmvfwmzdfjlvtqnbhcprsg", 29),
            new("zcfzfwzzqfrljwzlrfnpqdbhtmscgvjw", 26),
        });

    [Fact]
    public override async Task Part2_Actual()
        => await InvokeActual(Part.Two, 14);

    private void InvokeExample(Part part, int chunkSize, IEnumerable<TestData> testData)
    {
        foreach (var test in testData)
        {
            Invoke(part, test.Input, chunkSize, test.Expected);
        }
    }

    private async Task InvokeActual(Part part, int chunkSize, int? expected = null)
    {
        var data = await GetData(DataType.Actual);
        if (data.Length != 1) throw new InvalidOperationException($"Expected data to contain just a single item, found {data.Length}");
        Invoke(part, data.First(), chunkSize, expected);
    }

    private void Invoke(Part part, string data, int chunkSize, int? expected = null)
    {
        var signal = new CommunicationSignal(data, chunkSize);
        var device = new CommunicationDevice(signal);
        var result = device.DetectSignalStartMarker();

        WriteResult(part, result);

        if (expected.HasValue)
        {
            result.Should().Be(expected);
        }
    }

    private class CommunicationSignal : List<char>
    {
        public int ChunkSize { get; }

        public CommunicationSignal(string dataStream, int chunkSize)
        {
            AddRange(dataStream.ToCharArray());
            ChunkSize = chunkSize;
        }

        public IEnumerable<DataChunk> GetDataChunks()
        {
            for (int i = 0; i < Count - ChunkSize; i++)
            {
                yield return new DataChunk(GetRange(i, ChunkSize), i, i + ChunkSize);
            }
        }
    }

    private class DataChunk : List<char>
    {
        public int StartsAt { get; set; }
        public int EndsAt { get; set; }

        public DataChunk(List<char> data, int startsAt, int endsAt)
        {
            AddRange(data);
            StartsAt = startsAt;
            EndsAt = endsAt;
        }
    }

    private class CommunicationDevice
    {
        private readonly CommunicationSignal _signal;
        public CommunicationDevice(CommunicationSignal signal)
        {
            _signal = signal;
        }

        public int DetectSignalStartMarker() => _signal.GetDataChunks().First(s => s.Distinct().Count() == _signal.ChunkSize).EndsAt;
    }
}