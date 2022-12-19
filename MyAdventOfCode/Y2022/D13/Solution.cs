using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using MyAdventOfCode.Common;
using MyAdventOfCode.Common.Attributes;
using MyAdventOfCode.Extensions;

using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2022.D13;

public class Solution : TestBase
{
    public Solution(ITestOutputHelper output) : base(output)
    { }

    [Fact]
    public override async Task Part1_Example()
        => await Invoke(Part.One, DataType.Example, data => DistressSignal.Parse(data).SumCorrectIndices(), 13);

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke(Part.One, DataType.Actual, data => DistressSignal.Parse(data).SumCorrectIndices(), 6235);

    [Fact]
    public override async Task Part2_Example()
        => await Invoke_Part2(DataType.Example, 140);

    [Fact]
    public override async Task Part2_Actual()
        => await Invoke_Part2(DataType.Actual);

    private async Task Invoke_Part2(DataType dataType, int? expected = null)
    {
        await Invoke(Part.Two, dataType, data =>
        {
            var signal = DistressSignal2.Parse(data);
            var include = new[]
            {
                DataPacket.Parse(signal.Count +1, "[[2]]"),
                DataPacket.Parse(signal.Count +1, "[[6]]"),
            };
            signal.SortDataPackets(include);
            int result = signal.FindAndMultiplyIndices(include);
            return result;
        }, expected);
    }

    private async Task Invoke(Part part, DataType dataType, Func<string[], int> sut, int? expected = null)
    {
        var data = await GetData(dataType);
        var result = sut.Invoke(data);

        WriteResult(part, result);

        if (expected != null)
        {
            result.Should().Be(expected);
        }
    }

    private class DistressSignal : List<DataPacketPair>
    {
        public static DistressSignal Parse(string[] input)
        {
            var signal = new DistressSignal();
            int pairNo = 1, packetNo = 1;
            foreach (var chunk in input.Chunk(3))
            {
                var left = DataPacket.Parse(packetNo++, chunk[0]);
                var right = DataPacket.Parse(packetNo++, chunk[1]);
                signal.Add(new DataPacketPair(pairNo++, left, right));
            }
            return signal;
        }

        public int SumCorrectIndices() => this.Where(x => x.OrderValid()).Sum(x => x.Number);
    }

    private class DistressSignal2 : List<DataPacket>
    {
        public static DistressSignal2 Parse(string[] input)
        {
            var signal = new DistressSignal2();
            int packetNo = 1;
            foreach (var chunk in input.Chunk(3))
            {
                signal.Add(DataPacket.Parse(packetNo++, chunk[0]));
                signal.Add(DataPacket.Parse(packetNo++, chunk[1]));
            }
            return signal;
        }

        public DistressSignal2 SortDataPackets(params DataPacket[] include)
        {
            AddRange(include);
            Sort(DataPacket.CompareLists);
            return this;
        }

        public int FindAndMultiplyIndices(DataPacket[] include)
        {
            var total = 1;
            foreach (var packet in include)
            {
                var i = IndexOf(packet);
                if (i >= 0) total *= i + 1;
            }
            return total;
        }
    }

    private class DataPacketPair
    {
        public DataPacket Left { get; }
        public DataPacket Right { get; }
        public int Number { get; }

        public DataPacketPair(int number, DataPacket left, DataPacket right)
        {
            Left = left;
            Right = right;
            Number = number;
        }

        public bool OrderValid() => DataPacket.CompareLists(Left, Right) <= 0;
    }

    private class DataPacket : List<object>
    {
        public const char Separator = ',';
        public const char ArrayStart = '[';
        public const char ArrayEnd = ']';
        public int PacketNo { get; }
        public DataPacket(int packetNo, List<object> dataChunks)
        {
            PacketNo = packetNo;
            AddRange(dataChunks);
        }

        /// <summary>
        /// Kudos to JCollard: <see href="https://github.com/jcollard/AdventOfCode2022/blob/main/Day13-Guide/Solution/ListParser.cs"/>
        /// </summary>
        public static DataPacket Parse(int packetNo, string input)
        {
            var queuedData = new Queue<char>(input);
            var parsedData = ParseList(queuedData);
            return new DataPacket(packetNo, parsedData);
        }

        public static int CompareLists(List<object> first, List<object> second)
        {
            int maxIx = Math.Min(first.Count, second.Count);
            for (int ix = 0; ix < maxIx; ix++)
            {
                object el0 = first[ix];
                object el1 = second[ix];
                int diff = CompareElements(el0, el1);
                if (diff < 0)
                {
                    return -1;
                }
                else if (diff > 0)
                {
                    return 1;
                }
            }
            return Math.Sign(first.Count - second.Count);
        }

        private static int CompareElements(object first, object second)
        {
            return (first, second) switch
            {
                (int f, int s) => Math.Sign(f - s),
                (int f, List<object> s) => CompareLists(new List<object>() { f }, s),
                (List<object> f, int s) => CompareLists(f, new List<object>() { s }),
                (List<object> f, List<object> s) => CompareLists(f, s),
                _ => throw new Exception($"Could not compare elements {first} vs. {second}."),
            };
        }

        private static List<object> ParseList(Queue<char> data)
        {
            List<object> elements = new();
            data.Dequeue(); // Remove the '[' from the queue
            while (data.Peek() != ArrayEnd)
            {
                if (data.Peek() == Separator)
                {
                    data.Dequeue();
                }
                object el = ParseElement(data);
                elements.Add(el);
            }
            data.Dequeue(); // Remove the ']' from the queue
            return elements;
        }

        private static object ParseElement(Queue<char> data)
        {
            char next = data.Peek();
            if (char.IsDigit(next))
            {
                return ParseInt(data);
            }
            if (next == ArrayStart)
            {
                return ParseList(data);
            }
            throw new Exception($"Expected an int or list but found: {string.Join("", data)}");
        }

        private static int ParseInt(Queue<char> data)
        {
            var number = string.Empty;
            while (char.IsDigit(data.Peek()))
            {
                number += data.Dequeue();
            }
            return int.Parse(number);
        }
    }
}
