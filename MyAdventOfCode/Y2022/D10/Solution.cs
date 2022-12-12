using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using MyAdventOfCode.Common;

using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2022.D10;

public class Solution : TestBase
{
    public Solution(ITestOutputHelper output) : base(output)
    { }

    [Fact]
    public override async Task Part1_Example()
        => await Invoke(Part.One, DataType.Example, cpu => cpu.SignalStrength, 13140);

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke(Part.One, DataType.Actual, cpu => cpu.SignalStrength, 12640);

    [Fact]
    public override async Task Part2_Example()
        => await Invoke(Part.Two, DataType.Example, cpu => cpu.Display());

    [Fact]
    public override async Task Part2_Actual()
        => await Invoke(Part.Two, DataType.Actual, cpu => cpu.Display(),
@"####.#..#.###..####.#....###....##.###..
#....#..#.#..#....#.#....#..#....#.#..#.
###..####.###....#..#....#..#....#.#..#.
#....#..#.#..#..#...#....###.....#.###..
#....#..#.#..#.#....#....#.#..#..#.#.#..
####.#..#.###..####.####.#..#..##..#..#.");

    private async Task Invoke<T>(Part part, DataType dataType, Func<Processor, T> sut, T expected = default)
    {
        var data = await GetData(dataType);
        var instructions = SignalFactory.CreateManyFromRaw(data);

        var processor = new Processor(instructions);
        processor.Process();

        var result = sut.Invoke(processor);

        WriteResult(part, result);

        if (expected != null)
        {
            result.Should().Be(expected);
        }
    }


    private class SignalFactory
    {
        private static readonly Dictionary<string, Func<string[], Signal>> Map = new()
        {
            { NoOpSignal.Text, NoOpSignal.Parse },
            { AddXSignal.Text, AddXSignal.Parse }
        };

        public static IEnumerable<Signal> CreateManyFromRaw(string[] input)
        {
            foreach (var line in input)
            {
                yield return CreateFromRaw(line);
            }
        }

        private static Signal CreateFromRaw(string input)
        {
            var parts = input.Split();

            return Map.TryGetValue(parts.First(), out var parser)
                ? parser.Invoke(parts)
                : throw new InvalidOperationException("Signal not recognized");
        }
    }

    private interface ISignalParser<T> where T : ISignalParser<T>
    {
        public static abstract string Text { get; }
        public static abstract T Parse(params string[] input);
    }

    private enum SignalType { AddX, NoOp };
    private class Signal
    {
        public SignalType SignalType { get; }
        public int CyclesToComplete { get; }
        public int Value { get; }
        public int CyclesPassed { get; private set; }
        public bool Complete { get; private set; }

        public Signal(SignalType type, int cyclesToComplete, int value)
        {
            SignalType = type;
            CyclesToComplete = cyclesToComplete;
            Value = value;
        }

        public void RegisterCycle() => CyclesPassed++;
        public bool ReadyForExecution() => CyclesPassed == CyclesToComplete;
        public void MarkAsComplete() => Complete = true;

        public override string ToString()
            => $"{SignalType}{(Value == 0 ? "" : Value)}";
    }

    private class NoOpSignal : Signal, ISignalParser<NoOpSignal>
    {
        public static string Text => "noop";
        public NoOpSignal() : base(SignalType.NoOp, 1, 0) { }

        public static NoOpSignal Parse(params string[] input) => new();
    }

    private class AddXSignal : Signal, ISignalParser<AddXSignal>
    {
        public static string Text => "addx";
        public AddXSignal(int xValue) : base(SignalType.AddX, 2, xValue) { }

        public static AddXSignal Parse(params string[] input) => new(int.Parse(input.Last()));
    }

    private class Display
    {
        private const char ActivePixel = '#';
        private const char InactivePixel = '.';
        private readonly List<List<char>> _pixels = new();
        public Display(int width, int height)
        {
            for (int row = 0; row < height; row++)
            {
                _pixels.Add(new List<char>());
                for (int col = 0; col < width; col++)
                {
                    _pixels[row].Add(InactivePixel);
                }
            }
        }

        public void ActivatePixel(int row, int col)
        {
            _pixels[row][col] = ActivePixel;
        }

        public string RenderDisplay()
        {
            var text = string.Join(Environment.NewLine, _pixels.Select(x => string.Join(string.Empty, x)));
            Console.WriteLine(text);
            return text;
        }
    }

    private class Processor
    {
        public int Register { get; private set; } = 1;
        public int SignalStrength => _registerSnapshots.Sum(x => x.SignalValue);

        private readonly Display _display;
        private readonly Queue<Signal> _pending;
        private readonly List<Signal> _inflight;
        private readonly List<(int CycleNo, int SignalValue)> _registerSnapshots = new();
        private int _currentCycle;
        private int CurrentCycle
        {
            get => _currentCycle;
            set
            {
                if ((value - 20) % 40 == 0)
                    _registerSnapshots.Add((value, value * Register));

                _currentCycle = value;
            }
        }
        public Processor(IEnumerable<Signal> signals)
        {
            _pending = new Queue<Signal>(signals);
            _inflight = new List<Signal>();
            _display = new(40, 6);
        }

        public void Process()
        {
            while (_pending.Any() || _inflight.Any())
            {
                Console.WriteLine($"Cycle: {CurrentCycle + 1}, register: {Register}");

                foreach (var signal in _inflight)
                {
                    signal.RegisterCycle();

                    if (signal.ReadyForExecution())
                    {
                        Register += signal.Value;
                        signal.MarkAsComplete();
                        Console.WriteLine($"\tSignal Complete: {signal}, register: {Register}");
                    }
                }

                _inflight.RemoveAll(i => i.Complete);

                if (!_inflight.Any() && _pending.Any())
                {
                    var signal = _pending.Dequeue();
                    Console.WriteLine($"\tSignal Started: {signal}, register: {Register}");
                    _inflight.Add(signal);
                }

                var row = CurrentCycle / 40;
                var col = CurrentCycle % 40;
                if (col == Register - 1 || col == Register || col == Register + 1)
                {
                    _display.ActivatePixel(row, col);
                }

                CurrentCycle++;
            }
        }

        public string Display() => _display.RenderDisplay();
    }
}
