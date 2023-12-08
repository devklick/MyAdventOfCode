using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using FluentAssertions;

using MyAdventOfCode.Common;
using MyAdventOfCode.Common.Attributes;
using MyAdventOfCode.Extensions;

using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2023.D08;

public partial class Solution : TestBase
{
    private static ConsoleWriter _writer;
    public Solution(ITestOutputHelper output) : base(output)
    {
        _writer = new ConsoleWriter(_output);
    }

    [Fact]
    public override async Task Part1_Example()
        => await Invoke_Part1(
            DataType.Example,
            6);

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke_Part1(
            DataType.Actual,
            12083);

    [Fact]
    public override async Task Part2_Example()
        => await Invoke_Part2(
            DataType.Example,
            6);

    [Fact]
    public override async Task Part2_Actual()
        => await Invoke_Part2(
            DataType.Actual,
            21039729);

    private async Task Invoke_Part1(DataType dataType, int? expected = null)
        => await Invoke(Part.One, dataType, null, null, expected);

    private async Task Invoke_Part2(DataType dataType, int? expected = null)
        => await Invoke(Part.One, dataType, (n) => n.Name.EndsWith('A'), (n) => n.Name.EndsWith('Z'), expected);

    private async Task Invoke(Part part, DataType dataType, Func<Node, bool> start, Func<Node, bool> end, int? expected = null)
    {
        var data = await GetData(dataType, part);
        var nav = CircularNav.Parse(data);
        var network = Network.Parse(data, start, end);

        var result = network.NavigateToEnd(nav);

        WriteResult(part, result);

        if (expected != null)
        {
            result.Should().Be(expected, part.ToString());
        }
    }
    private enum Direction
    {
        [Alias("L")]
        Left,

        [Alias("R")]
        Right
    }

    private class CircularNav
    {
        private static readonly Dictionary<string, Direction> DirMap =
            EnumHelper.GetEnumMembersAndAttributes<Direction, AliasAttribute>()
            .ToDictionary(e => e.Value.Aliases.First(), e => e.Key);

        private readonly List<Direction> _directions;
        private int _currentIndex = 0;
        public CircularNav(IEnumerable<Direction> directions)
        {
            _directions = directions.ToList();
        }

        public static CircularNav Parse(string[] data)
            => new(data.First().Select(c => DirMap[c.ToString()]));

        public IEnumerable<Direction> EnumerateUntil(Func<bool> condition)
        {
            while (!condition())
            {
                var next = _directions[_currentIndex];
                _currentIndex++;
                if (_currentIndex > _directions.Count - 1)
                {
                    _currentIndex = 0;
                }
                yield return next;
            }
        }
    }

    private class Node
    {
        public string Name { get; }
        public Dictionary<Direction, Node> Neighbors { get; }

        public Node(string name)
        {
            Name = name;
            Neighbors = new Dictionary<Direction, Node>();
        }

        // public override string ToString()
        //     => $"{Name} = ({Neighbors[Direction.Left]},{Neighbors[Direction.Right]})";

        public void AddNeighbor(Direction direction, Node neighbor)
            => Neighbors[direction] = neighbor;

        public Node GetNeighbor(Direction direction)
            => Neighbors[direction];
    }

    private partial class Network
    {
        [GeneratedRegex("[^A-Z.]", RegexOptions.Compiled)]
        private static partial Regex NodeNameRegex();

        public Dictionary<string, Node> Nodes { get; }
        public List<Node> StartNodes;
        public List<Node> End;
        public List<Node> Current { get; private set; }

        public Network(Dictionary<string, Node> nodes, Func<Node, bool> identifyStartNodes = null, Func<Node, bool> identifyEndNodes = null)
        {
            Nodes = nodes;
            StartNodes = FindMatchingNodes(identifyStartNodes, "AAA");
            End = FindMatchingNodes(identifyEndNodes, "ZZZ");
            Current = new List<Node>(StartNodes);
        }

        public int NavigateToEnd(CircularNav navigation)
        {
            var steps = 0;
            foreach (var direction in navigation.EnumerateUntil(() => Current.SequenceEqual(End)))
            {
                for (var c = 0; c < Current.Count; c++)
                {
                    var current = Current[c];
                    var newCurrent = current.GetNeighbor(direction);
                    Current.Replace(current, newCurrent);
                }
                steps++;
            }
            return steps;
        }

        private List<Node> FindMatchingNodes(Func<Node, bool> condition, string defaultNodeName)
        {
            return condition != null
                ? Nodes.Where(x => condition(x.Value)).Select(n => n.Value).ToList()
                : new List<Node> { Nodes[defaultNodeName] };
        }

        public static Network Parse(string[] data, Func<Node, bool> identifyStartNodes = null, Func<Node, bool> identifyEndNodes = null)
        {
            var nodes = new Dictionary<string, Node>();

            foreach (var row in data.Skip(2))
            {
                var parts = row.Split(" = ");
                var name = parts.First();
                var neighbors = parts.Last().Split(", ").Select(n => NodeNameRegex().Replace(n, ""));

                if (!nodes.TryGetValue(name, out var node))
                {
                    node = new Node(name);
                    nodes.Add(name, node);
                }

                neighbors.ToList().ForEach((n, i) =>
                {
                    if (!nodes.TryGetValue(n, out var neighbor))
                    {
                        neighbor = new Node(n);
                        nodes.Add(n, neighbor);
                    }

                    node.AddNeighbor(i == 0 ? Direction.Left : Direction.Right, neighbor);
                });
            }

            return new Network(nodes, identifyStartNodes, identifyEndNodes);
        }
    }

}
