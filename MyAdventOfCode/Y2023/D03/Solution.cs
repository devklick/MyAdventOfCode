using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using MyAdventOfCode.Common;
using MyAdventOfCode.Extensions;

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
            (schematic) => schematic.PartNos.Sum(x => x.Value),
            4361);

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke(
            Part.One,
            DataType.Actual,
            (schematic) => schematic.PartNos.Sum(x => x.Value),
            537732);

    /*
        Part 2 is far from optimal, but it's easier easier than refactoring 
        the solution for Part 1 to factor in these new requirements.
    */
    [Fact]
    public override async Task Part2_Example()
        => await Invoke(
            Part.Two,
            DataType.Example,
            (schematic) => schematic.Gears.Sum(x => x.Ratio),
            467835);

    [Fact]
    public override async Task Part2_Actual()
        => await Invoke(
            Part.Two,
            DataType.Actual,
            (schematic) => schematic.Gears.Sum(x => x.Ratio),
            0);

    private async Task Invoke(Part part, DataType dataType, Func<EngineSchematic, int> getResult, int? expected = null)
    {
        var data = await GetData(dataType, part);
        var partNos = new PartNoParser(data).GetPartNos();
        var schematic = new EngineSchematic(partNos);
        var result = getResult(schematic);

        WriteResult(part, result);

        if (expected != null)
        {
            result.Should().Be(expected, part.ToString());
        }
    }


    private class EngineSchematic
    {
        public List<PartNo> PartNos { get; }
        public List<Gear> Gears { get; private set; }


        public EngineSchematic(List<PartNo> partNos)
        {
            PartNos = partNos;
            Gears = GetGears();
        }

        private List<Gear> GetGears()
        {
            var gears = new List<Gear>();

            // This is terribly inefficient. It loops through the entire 
            // list of part numbers several times. I should really go with 
            // another approach, but this involves the least amount of reworking
            // to the solution of part 1.
            for (var i = 0; i < PartNos.Count; i++)
            {
                var partNo = PartNos[i];

                // If we've already counted this one as part of another gear, skip it
                if (gears.Any(g => g.PartNo1 == partNo || g.PartNo2 == partNo))
                {
                    continue;
                }

                foreach (var otherPartNo in PartNos)
                {
                    // Avoid matching on self
                    if (otherPartNo == partNo) continue;

                    // If the two parts share the same position, it's a gear
                    if (partNo.Symbol.Position == otherPartNo.Symbol.Position)
                    {
                        gears.Add(new Gear
                        {
                            PartNo1 = partNo,
                            PartNo2 = otherPartNo
                        });
                    }
                }

            }
            return gears;
        }
    }

    private class Gear
    {
        public PartNo PartNo1 { get; set; }
        public PartNo PartNo2 { get; set; }
        public int Ratio => PartNo1.Value * PartNo2.Value;
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