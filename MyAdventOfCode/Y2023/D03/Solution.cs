using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using MyAdventOfCode.Common;

using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2023.D03;

public class Solution : TestBase
{
    public Solution(ITestOutputHelper output) : base(output)
    { }

    [Fact]
    public override async Task Part1_Example()
        => await Invoke(
            Part.One,
            DataType.Example,
            4361);

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke(
            Part.One,
            DataType.Actual,
            537732);

    [Fact]
    public override async Task Part2_Example()
        => await Invoke(
            Part.Two,
            DataType.Actual,
            0);

    [Fact]
    public override async Task Part2_Actual()
        => await Invoke(
            Part.Two,
            DataType.Actual,
            0);

    record Game(int Num, IReadOnlyList<Set> Sets);

    record Set(int Red, int Green, int Blue);

    private async Task Invoke(Part part, DataType dataType, int? expected = null)
    {
        var data = await GetData(dataType, part);
        var partNos = new PartNoParser(data).GetPartNos();
        var result = partNos.Sum(x => x.Value);

        WriteResult(part, result);

        if (expected != null)
        {
            result.Should().Be(expected, part.ToString());
        }
    }

    private class EngineSchematic
    {
        private readonly List<PartNo> _partNos;

        public EngineSchematic(List<PartNo> partNos)
        {
            _partNos = partNos;
        }
    }

    private class PartNo
    {
        public int Value { get; set; }
        public (RCVector Start, RCVector End) Position { get; set; }
        public (char Value, RCVector Position) Symbol { get; set; }
    }

    private class PartNoParser
    {
        private readonly string[] _data;

        public PartNoParser(string[] data)
        {
            _data = data;
        }

        public List<PartNo> GetPartNos()
        {
            var partNos = new List<PartNo>();
            for (var r = 0; r < _data.Length; r++)
            {
                for (var c = 0; c < _data[r].Length; c++)
                {
                    var partNo = GetPartNoStartingFrom(r, c);

                    if (partNo != null)
                    {
                        c = partNo.Position.End.Column;
                        partNos.Add(partNo);
                    }
                }
            }
            return partNos;
        }

        private PartNo GetPartNoStartingFrom(int startRow, int startColumn)
        {
            for (var c = startColumn; c < _data[startRow].Length; c++)
            {
                var lookAheadResult = LookAheadAndGetNumber(startRow, c);

                if (!lookAheadResult.HasValue) continue;

                var (number, endColumn) = lookAheadResult.Value;
                var startPosition = new RCVector(startRow, c);
                var endPosition = new RCVector(startRow, endColumn);

                c = endColumn;

                var getSymbolResult = GetFirstAdjacentSymbol(
                    startPosition,
                    endPosition
                );

                if (!getSymbolResult.HasValue) continue;

                return new PartNo
                {
                    Position = (startPosition, endPosition),
                    Symbol = getSymbolResult.Value,
                    Value = number
                };
            }

            return null;
        }

        private (int Number, int EndColumn)? LookAheadAndGetNumber(int row, int startColumn)
        {
            string value = string.Empty;

            for (var col = startColumn; col < _data[row].Length; col++)
            {
                var character = _data[row][col];

                if (char.IsDigit(character))
                {
                    value = string.Concat(value, character);
                }
                else
                {
                    return string.IsNullOrEmpty(value)
                        ? null
                        : (int.Parse(value), col - 1);
                }
            }

            return string.IsNullOrEmpty(value)
                ? null
                : (int.Parse(value), _data[row].Length - 1);
        }

        private (char Symbol, RCVector Position)? GetFirstAdjacentSymbol(RCVector from, RCVector to)
        {
            // Warning: Switching from RC position to XY position here
            var border = new Rectangle(from.ToVector(), to.ToVector()).GetBorderPositions();

            foreach (var borderPosition in border)
            {
                var inBounds = borderPosition.X >= 0 && borderPosition.X < _data.Length
                    && borderPosition.Y >= 0 && borderPosition.Y < _data[borderPosition.X].Length;

                if (!inBounds) continue;

                var rcPosition = borderPosition.ToRCVector();
                var character = _data[rcPosition.Row][rcPosition.Column];

                if (IsValidSymbol(character))
                {
                    return (character, rcPosition);
                }
            }
            return null;
        }

        private static bool IsValidSymbol(char value)
            => !char.IsLetterOrDigit(value) && value != '.';
    }
}