using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using MyAdventOfCode.Common;

using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2022.D08;

public class Solution : TestBase
{
    public Solution(ITestOutputHelper output) : base(output)
    { }

    [Fact]
    public override async Task Part1_Example()
        => await Invoke_Part1(DataType.Example, 21);

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke_Part1(DataType.Actual, 1807);

    [Fact]
    public override async Task Part2_Example()
        => await Invoke_Part2(DataType.Example, 8);


    [Fact]
    public override async Task Part2_Actual()
        => await Invoke_Part2(DataType.Actual, 480000);

    private async Task Invoke_Part1(DataType dataType, int? expected = null)
        => await Invoke(
            Part.Two,
            dataType,
            forrest => forrest.GetVisibleTrees(RCVector.Cardinals).Count(),
            expected);

    private async Task Invoke_Part2(DataType dataType, int? expected = null)
        => await Invoke(
            Part.Two,
            dataType,
            forrest => forrest.GetTreesVisibleFromTree(RCVector.Cardinals).Max(tree => tree.CanSee.Aggregate(1, (cur, next) => cur * next.Trees.Count())),
            expected);

    private async Task Invoke(Part part, DataType dataType, Func<Forrest, int> sut, int? expected = null)
    {
        var data = await GetData(dataType);
        var forrest = new Forrest(data);
        var result = sut.Invoke(forrest);

        WriteResult(part, result);

        if (expected.HasValue)
        {
            result.Should().Be(expected);
        }
    }

    private class Tree
    {
        public RCVector Position { get; }
        public int Height { get; }

        public Tree(int row, int column, int height)
        {
            Position = new RCVector(row, column);
            Height = height;
        }
    }

    private class Forrest : List<List<Tree>>
    {
        public int Width => this.FirstOrDefault()?.Count ?? 0;
        public int Length => Count;

        public Forrest(string[] treeMap)
        {
            for (var row = 0; row < treeMap.Length; row++)
            {
                Add(new List<Tree>());
                var treeHeights = treeMap[row].ToCharArray().Select(c => int.Parse(c.ToString())).ToList();

                for (var col = 0; col < treeHeights.Count; col++)
                {
                    this[row].Add(new Tree(row, col, treeHeights[col]));
                }
            }
        }

        public IEnumerable<Tree> GetVisibleTrees(params RCVector[] directions)
        {
            foreach (var row in this)
            {
                foreach (var tree in row)
                {
                    if (IsOnEdge(tree))
                    {
                        yield return tree;
                        continue;
                    }

                    foreach (var direction in directions)
                    {
                        if (IsVisibleFrom(tree, direction))
                        {
                            yield return tree;
                            break;
                        }
                    }
                }
            }
        }

        public IEnumerable<(Tree Tree, IEnumerable<(RCVector Direction, IEnumerable<Tree> Trees)> CanSee)> GetTreesVisibleFromTree(params RCVector[] viewDirections)
        {
            foreach (var row in this)
            {
                foreach (var tree in row)
                {
                    var canSee = new List<(RCVector, IEnumerable<Tree>)>();
                    foreach (var direction in viewDirections)
                    {
                        var treesVisibleByDirection = GetTreesVisibleFromTree(tree, direction).ToList();
                        canSee.Add((direction, treesVisibleByDirection));
                    }
                    yield return (tree, canSee);
                }
            }
        }

        private IEnumerable<Tree> GetTreesVisibleFromTree(Tree tree, RCVector direction)
        {
            if (IsOnEdge(tree, direction))
            {
                yield break;
            }

            var otherTree = this[tree.Position.Row + direction.Row][tree.Position.Column + direction.Column];

            while (otherTree != null)
            {
                if (otherTree.Height >= tree.Height)
                {
                    yield return tree;
                    break;
                }

                yield return tree;
                var otherRow = otherTree.Position.Row + direction.Row;
                var otherCol = otherTree.Position.Column + direction.Column;
                otherTree = this.ElementAtOrDefault(otherRow)?.ElementAtOrDefault(otherCol);
            }
        }

        private bool IsVisibleFrom(Tree tree, RCVector direction)
        {
            var otherTree = this[tree.Position.Row + direction.Row][tree.Position.Column + direction.Column];
            while (otherTree != null)
            {
                if (otherTree.Height >= tree.Height) return false;

                var otherRow = otherTree.Position.Row + direction.Row;
                var otherCol = otherTree.Position.Column + direction.Column;
                otherTree = this.ElementAtOrDefault(otherRow)?.ElementAtOrDefault(otherCol);
            }
            return true;
        }

        private bool IsOnEdge(Tree tree)
            => tree.Position.Row == 0 || tree.Position.Column == 0
            || tree.Position.Row == Width - 1 || tree.Position.Column == Length - 1;

        private bool IsOnEdge(Tree tree, RCVector direction) => direction switch
        {
            NorthRCVector => tree.Position.Row == 0,
            EastRCVector => tree.Position.Column == Width - 1,
            SouthRCVector => tree.Position.Row == Length - 1,
            WestRCVector => tree.Position.Column == 0,
            _ => throw new NotImplementedException("Direction not supported")
        };
    }
}