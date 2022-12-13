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

namespace MyAdventOfCode.Y2022.D11;

public class Solution : TestBase
{
    public Solution(ITestOutputHelper output) : base(output)
    { }

    [Fact]
    public override async Task Part1_Example()
        => await Invoke(Part.One, DataType.Example, 20, worry => worry / 3, 10605);

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke(Part.One, DataType.Actual, 20, worry => worry / 3, 120056);

    [Fact]
    public override async Task Part2_Example()
        => await Invoke_Part2(DataType.Example, 2713310158);

    [Fact]
    public override async Task Part2_Actual()
        => await Invoke_Part2(DataType.Actual, 21816744824);

    private async Task Invoke_Part2(DataType dataType, long? expected = null)
    {
        var data = await GetData(dataType);
        var monkeys = Monkey.ParseMany(data).ToList();
        var worryFactor = monkeys.Select(monkey => monkey.TestDivisor).Aggregate((a, b) => a * b);
        await Invoke(Part.Two, dataType, 10000, worry => worry % worryFactor, expected);
    }

    private async Task Invoke(Part part, DataType dataType, int observeForN, Func<long, long> worryManagement, long? expected = null)
    {
        var data = await GetData(dataType);
        var monkeys = Monkey.ParseMany(data).ToList();
        var monitor = new MonkeyMonitor(monkeys);

        monitor.ObserveFor(observeForN, worryManagement);
        var result = monitor.CalculateMonkeyBusiness();

        WriteResult(part, result);

        if (expected != null)
        {
            result.Should().Be(expected);
        }
    }

    private class Item
    {
        public long WorryLevel { get; private set; }
        public Item(int worryLevel)
        {
            WorryLevel = worryLevel;
        }

        public void Inspect(Operation worryLevelModifierOperation, long worryLevelModifierValue) => WorryLevel = worryLevelModifierOperation switch
        {
            Operation.Multiply => WorryLevel * worryLevelModifierValue,
            Operation.Plus => WorryLevel + worryLevelModifierValue,
            _ => throw new NotImplementedException("Operation not supported"),
        };

        public void PostInspection(Func<long, long> worryManagement)
            => WorryLevel = worryManagement.Invoke(WorryLevel);
    }

    private class Monkey
    {
        public int Number { get; private set; }
        public Queue<Item> Items { get; private set; } = new();
        public List<Item> ItemsInspected { get; private set; } = new();
        public Operation WorryLevelModifierOperation { get; private set; }
        public int? WorryLevelModifierValue { get; private set; }
        public int TestDivisor { get; private set; }
        public Dictionary<bool, int> TestResultAction { get; private set; } = new();

        public IEnumerable<(Item Item, int TargetMonkeyNumber)> InspectAndThrowItems(Func<long, long> worryManagement)
        {
            while (Items.Any())
            {
                var item = Items.Dequeue();
                InspectItem(item, worryManagement);
                var targetMonkeyNumber = TestResultAction[ExecuteTest(item)];
                yield return (item, targetMonkeyNumber);
            }
        }

        private void InspectItem(Item item, Func<long, long> worryManagement)
        {
            item.Inspect(WorryLevelModifierOperation, WorryLevelModifierValue ?? item.WorryLevel);
            ItemsInspected.Add(item);
            item.PostInspection(worryManagement);
        }

        public void CatchItem(Item item)
            => Items.Enqueue(item);

        private bool ExecuteTest(Item item)
            => item.WorryLevel % TestDivisor == 0;

        public static IEnumerable<Monkey> ParseMany(string[] input)
        {
            var lines = new List<string>();
            foreach (var line in input)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    yield return Parse(lines);
                    lines = new();
                }
                else
                {
                    lines.Add(line);
                }
            }
            yield return Parse(lines);
        }

        private static Monkey Parse(IEnumerable<string> input)
        {
            if (input.Count() != 6)
                throw new InvalidDataException("Expected input to contain 6 rows.");

            return new Monkey
            {
                Number = ParseMonkeyNumber(input.First()),
                Items = ParseItems(input.ElementAt(1)),
                WorryLevelModifierOperation = ParseModifierOperation(input.ElementAt(2)),
                WorryLevelModifierValue = ParseModifierValue(input.ElementAt(2)),
                TestDivisor = ParseTestDivisor(input.ElementAt(3)),
                TestResultAction = ParseTestResultAction(input.ElementAt(4), input.ElementAt(5)),
            };
        }

        private static Dictionary<bool, int> ParseTestResultAction(string value1, string value2)
            => new(new[] { ParseTestResultAction(value1), ParseTestResultAction(value2) });

        private static KeyValuePair<bool, int> ParseTestResultAction(string value)
        {
            var result = value.Contains(bool.TrueString.ToLower()) || (value.Contains(bool.FalseString.ToLower())
                ? false
                : throw new InvalidDataException("Expected input to contain either true or false"));

            var number = int.Parse(string.Join(string.Empty, value.Where(char.IsDigit)));

            return new KeyValuePair<bool, int>(result, number);
        }

        private static int ParseTestDivisor(string value)
            => int.Parse(string.Join(string.Empty, value.Where(char.IsDigit)));

        private static Queue<Item> ParseItems(string input)
            => new(string.Join(string.Empty, input.Where(c => char.IsDigit(c) || c == ',')).Split(',').Select(i => new Item(int.Parse(i))));

        private static int? ParseModifierValue(string value)
            => int.TryParse(string.Join(string.Empty, value.Where(char.IsDigit)), out var number) ? number : null;

        private static Operation ParseModifierOperation(string value)
            => value.Where(c => c == '+' || c == '*').Select(c => c == '+' ? Operation.Plus : Operation.Multiply).First();

        private static int ParseMonkeyNumber(string input)
            => int.TryParse(string.Join(string.Empty, input.Where(char.IsDigit)), out var monkeyNumber)
                ? monkeyNumber
                : throw new InvalidDataException("Unable to parse monkey number.");
    }

    private enum Operation
    {
        [Alias("+")] Plus,
        [Alias("*")] Multiply
    }

    private class MonkeyMonitor
    {
        private readonly Dictionary<int, Monkey> _monkeys;

        public MonkeyMonitor(IEnumerable<Monkey> monkeys)
        {
            _monkeys = monkeys.ToDictionary(x => x.Number, x => x);
        }

        public void ObserveFor(int times, Func<long, long> worryManagement)
        {
            for (int i = 0; i < times; i++)
            {
                foreach (var monkey in _monkeys.Values)
                {
                    foreach (var thrown in monkey.InspectAndThrowItems(worryManagement))
                    {
                        _monkeys[thrown.TargetMonkeyNumber].CatchItem(thrown.Item);
                    }
                }
            }
        }

        public long CalculateMonkeyBusiness()
            => _monkeys.Values
                .OrderBy(m => m.ItemsInspected.Count)
                .TakeLast(2)
                .Aggregate((long)1, (acc, cur) => acc * cur.ItemsInspected.Count);

    }
}
