using System;
using System.Collections.Generic;
using System.Linq;
using MyAdventOfCode.Common;
using MyAdventOfCode.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2015
{
    public class D14
    {
        private readonly ITestOutputHelper _output;
        public D14(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Part_1_Examples()
        {
            var input =
@"Comet can fly 14 km/s for 10 seconds, but then must rest for 127 seconds.
Dancer can fly 16 km/s for 11 seconds, but then must rest for 162 seconds.";

            var reindeer = ReindeerStats.ParseMultiple(input);
            var race = new RaceEmulator(reindeer);
            var results = race.RunFor(1000, Part.One);
            var comet = results.First(r => r.Reindeer.Name == "Comet");
            var dancer = results.First(r => r.Reindeer.Name == "Dancer");
            Assert.Equal(1, comet.Position);
            Assert.Equal(1120, comet.DistanceTraveled);
            Assert.Equal(2, dancer.Position);
            Assert.Equal(1056, dancer.DistanceTraveled);
        }

        [Fact]
        public void Part_1()
            => _output.WriteLine(Run_Part_X(Part.One).DistanceTraveled);

        [Fact]
        public void Part_2()
            => _output.WriteLine(Run_Part_X(Part.Two).Points);

        private RaceResult Run_Part_X(Part part)
        {
            var input =
@"Vixen can fly 8 km/s for 8 seconds, but then must rest for 53 seconds.
Blitzen can fly 13 km/s for 4 seconds, but then must rest for 49 seconds.
Rudolph can fly 20 km/s for 7 seconds, but then must rest for 132 seconds.
Cupid can fly 12 km/s for 4 seconds, but then must rest for 43 seconds.
Donner can fly 9 km/s for 5 seconds, but then must rest for 38 seconds.
Dasher can fly 10 km/s for 4 seconds, but then must rest for 37 seconds.
Comet can fly 3 km/s for 37 seconds, but then must rest for 76 seconds.
Prancer can fly 9 km/s for 12 seconds, but then must rest for 97 seconds.
Dancer can fly 37 km/s for 1 seconds, but then must rest for 36 seconds.";

            var reindeer = ReindeerStats.ParseMultiple(input);
            var race = new RaceEmulator(reindeer);
            var results = race.RunFor(2503, part);
            var winner = results.First(r => r.Position == 1);
            return winner;
        }

        private class RaceEmulator
        {
            private List<ReindeerState> _reindeerState;

            public RaceEmulator(IEnumerable<ReindeerStats> reindeerStats)
            {
                _reindeerState = new List<ReindeerState>();
                foreach (var deerStats in reindeerStats)
                {
                    _reindeerState.Add(new ReindeerState
                    {
                        ReindeerStats = deerStats
                    });
                }
            }

            public List<RaceResult> RunFor(int seconds, Part part)
            {
                foreach (var second in Enumerable.Range(1, seconds))
                {
                    foreach (var deer in _reindeerState)
                    {
                        deer.Second = second;

                        if (deer.State == ReindeerState.States.Flying)
                        {
                            deer.DistanceTraveled += deer.ReindeerStats.Speed.Rate;
                            deer.StateDuration++;
                            if (deer.StateDuration >= deer.ReindeerStats.Speed.Duration)
                            {
                                deer.State = ReindeerState.States.Resting;
                                deer.StateDuration = 0;
                            }
                        }
                        else // ReindeerState.States.Resting
                        {
                            deer.StateDuration++;
                            if (deer.StateDuration >= deer.ReindeerStats.Rest.Duration)
                            {
                                deer.State = ReindeerState.States.Flying;
                                deer.StateDuration = 0;
                            }
                        }
                    }
                    var currentWinningDistance = _reindeerState.Max(r => r.DistanceTraveled);
                    foreach (var deer in _reindeerState.Where(r => r.DistanceTraveled == currentWinningDistance))
                    {
                        deer.Points++;
                    }
                }

                var position = 1;
                var results = new List<RaceResult>();
                foreach (var deer in _reindeerState.OrderByDescending(r => part == Part.One ? r.DistanceTraveled : r.Points))
                {
                    results.Add(new RaceResult
                    {
                        DistanceTraveled = deer.DistanceTraveled,
                        Position = position,
                        Reindeer = deer.ReindeerStats,
                        Points = deer.Points
                    });
                    position++;
                }

                return results;
            }

            private class ReindeerState
            {
                public enum States { Flying, Resting };
                public ReindeerStats ReindeerStats { get; set; }
                public States State { get; set; }
                public int StateDuration { get; set; }
                public int DistanceTraveled { get; set; }
                public int Second { get; set; }
                public int Points { get; set; }

            }
        }

        private class RaceResult
        {
            public ReindeerStats Reindeer { get; set; }
            public int Position { get; set; }
            public int DistanceTraveled { get; set; }
            public int Points { get; set; }
        }

        private class ReindeerStats
        {
            public string Name { get; set; }
            public Stat Speed { get; set; }
            public Stat Rest { get; set; }

            public static ReindeerStats Parse(string template)
            {
                var parts = template.Split(' ');
                return new ReindeerStats
                {
                    Name = parts[0],
                    Speed = new Stat
                    {
                        Rate = int.Parse(parts[3]),
                        Duration = int.Parse(parts[6])
                    },
                    Rest = new Stat
                    {
                        Rate = 0,
                        Duration = int.Parse(parts[13])
                    }
                };
            }

            public static IEnumerable<ReindeerStats> ParseMultiple(string template)
            {
                foreach (var line in template.Split(Environment.NewLine))
                {
                    yield return Parse(line);
                }
            }
        }

        private class Stat
        {
            public int Rate { get; set; }
            public int Duration { get; set; }
        }
    }
}