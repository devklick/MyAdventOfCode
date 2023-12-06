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

namespace MyAdventOfCode.Y2023.D06;

public class Solution : TestBase
{
    public Solution(ITestOutputHelper output) : base(output)
    { }

    [Fact]
    public override async Task Part1_Example()
        => await Invoke(
            Part.One,
            DataType.Example,
            114400);

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke(
            Part.One,
            DataType.Actual,
            71503);

    [Fact]
    public override async Task Part2_Example()
        => await Invoke(
            Part.Two,
            DataType.Example,
            71503);

    [Fact]
    public override async Task Part2_Actual()
    => await Invoke(
        Part.Two,
        DataType.Actual,
        21039729);

    private async Task Invoke(Part part, DataType dataType, long? expected = null)
    {
        var data = await GetData(dataType, part);
        var joinNumbers = part == Part.Two;
        var stats = RaceStats.ParseMany(data, joinNumbers).ToList();
        var result = stats.Aggregate(1L, (total, stat) => total * stat.CalculateNumberOfWaysToWin());

        WriteResult(part, result);

        if (expected != null)
        {
            result.Should().Be(expected, part.ToString());
        }
    }

    private class RaceStats
    {
        public long BestTime { get; }
        public long BestDistance { get; }
        public RaceStats(long bestTime, long bestDistance)
        {
            BestTime = bestTime;
            BestDistance = bestDistance;
        }

        // Kudos to https://github.com/encse/adventofcode/blob/master/2023/Day06/Solution.cs
        // for giving a decent breakdown of the calculations of the quadratic formula
        public long CalculateNumberOfWaysToWin()
        {
            var quad = new QuadraticEquation(-1, BestTime, -BestDistance);
            var result = quad.SolveEquation();
            if (!result.HasValue) throw new Exception("Unable to solve quadratic equation");

            var (root1, root2) = result.Value;

            var maxX = (long)Math.Ceiling(root2) - 1;
            var minX = (long)Math.Floor(root1) + 1;

            return maxX - minX + 1;
        }

        public static IEnumerable<RaceStats> ParseMany(string[] data, bool joinNumbers)
        {
            var times = ExtractNumbers(data.First(), joinNumbers);
            var distances = ExtractNumbers(data.Last(), joinNumbers);

            for (var i = 0; i < times.Count(); i++)
            {
                var time = times.ElementAt(i);
                var distance = distances.ElementAt(i);

                yield return new RaceStats(time, distance);
            }
        }

        private static IEnumerable<long> ExtractNumbers(string text, bool join)
        {
            var numbers = text.Split().Where(part => !string.IsNullOrEmpty(part) && part.All(char.IsDigit)).Select(long.Parse);
            return join ? (new[] { long.Parse(string.Join("", numbers)) }) : numbers;
        }
    }

    // <summary>
    // Represents a quadratic equation of the form ax^2 + bx + c = 0.
    // The class is equipped to solve the equation and calculate the roots.
    //
    // Attributes:
    // - A: Represents the coefficient of the quadratic term (x^2) in the equation.
    // - B: Represents the coefficient of the linear term (x) in the equation.
    // - C: Represents the constant term in the equation.
    //
    // See https://codepal.ai/code-generator/query/rdOMKqIC/quadratic-equation-solver
    // </summary>
    public class QuadraticEquation
    {
        public long A { get; private set; }
        public long B { get; private set; }
        public long C { get; private set; }

        // <summary>
        // Constructs a new quadratic equation using the provided coefficients.
        //
        // Parameters:
        // - a: Coefficient for the quadratic term (x^2) in the equation.
        // - b: Coefficient for the linear term (x) in the equation.
        // - c: The constant term in the equation.
        // </summary>
        public QuadraticEquation(long a, long b, long c)
        {
            // Assign coefficients.
            A = a; // Set the quadratic coefficient.
            B = b; // Set the linear coefficient.
            C = c; // Set the constant term.
        }

        // <summary>
        // Solves the quadratic equation and calculates the roots.
        //
        // Returns:
        // - An array of doubles representing the roots of the equation.
        // - If the equation has two real roots, the array will contain both roots.
        // - If the equation has one real root, the array will contain that root twice.
        // - If the equation has no real roots, the array will be empty.
        // </summary>
        public (double root1, double root2)? SolveEquation()
        {
            double discriminant = B * B - 4 * A * C;
            double root1 = (-B + Math.Sqrt(discriminant)) / (2 * A);
            double root2 = (-B - Math.Sqrt(discriminant)) / (2 * A);
            return (root1, root2);

        }
    }
}
