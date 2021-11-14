using System;
using System.Collections.Generic;
using System.Linq;
using MyAdventOfCode.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2015
{
    public class D13
    {
        private readonly ITestOutputHelper _output;
        public D13(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Part_1_Example()
        {
            var data =
@"Alice would gain 54 happiness units by sitting next to Bob.
Alice would lose 79 happiness units by sitting next to Carol.
Alice would lose 2 happiness units by sitting next to David.
Bob would gain 83 happiness units by sitting next to Alice.
Bob would lose 7 happiness units by sitting next to Carol.
Bob would lose 63 happiness units by sitting next to David.
Carol would lose 62 happiness units by sitting next to Alice.
Carol would gain 60 happiness units by sitting next to Bob.
Carol would gain 55 happiness units by sitting next to David.
David would gain 46 happiness units by sitting next to Alice.
David would lose 7 happiness units by sitting next to Bob.
David would gain 41 happiness units by sitting next to Carol.";

            var map = NeighbourBasedHappinessMap.ParseMultiple(data);
            var planner = new GuestSeatingPlanner(map);
            var optimalSeating = planner.GetOptimalSeatingPlan();
            Assert.Equal(330, optimalSeating.OverallHappiness);
        }

        [Fact]
        public void Part_1()
        {
            var map = NeighbourBasedHappinessMap.ParseMultiple(InputData);
            Run_Part_X(map);
        }

        [Fact]
        public void Part_2()
        {
            var map = NeighbourBasedHappinessMap.ParseMultiple(InputData);
            foreach (var neighbor in map)
            {
                neighbor.Value.Add("Me", 0);
            }
            map.Add("Me", map.ToDictionary(m => m.Key, _ => 0));
            Run_Part_X(map);
        }

        private void Run_Part_X(NeighbourBasedHappinessMap map)
        {
            var planner = new GuestSeatingPlanner(map);
            var optimalSeating = planner.GetOptimalSeatingPlan();
            _output.WriteLine(optimalSeating.OverallHappiness);
        }

        private class GuestSeatingPlanner
        {
            private readonly NeighbourBasedHappinessMap _map;
            private readonly List<string> _guestList;
            public GuestSeatingPlanner(NeighbourBasedHappinessMap map)
            {
                _map = map;
                _guestList = new List<string>(_map.Keys);
            }

            public List<GuestSeatingPlan> GetSeatingPlans()
            {
                var seatingPlans = new List<GuestSeatingPlan>();
                foreach (var seatingOption in _guestList.GetPermutations())
                {
                    var happiness = 0;
                    for (var i = 0; i < seatingOption.Length; i++)
                    {
                        var leftIndex = i;
                        var rightIndex = leftIndex + 1;
                        if (rightIndex >= seatingOption.Length)
                        {
                            rightIndex = 0;
                        }

                        var leftGuest = seatingOption[leftIndex];
                        var rightGuest = seatingOption[rightIndex];

                        happiness += _map[leftGuest][rightGuest];
                        happiness += _map[rightGuest][leftGuest];
                    }

                    seatingPlans.Add(new GuestSeatingPlan
                    {
                        Guests = seatingOption.ToList(),
                        OverallHappiness = happiness
                    });
                }

                return seatingPlans;
            }
            public GuestSeatingPlan GetOptimalSeatingPlan()
                => GetSeatingPlans().OrderByDescending(s => s.OverallHappiness).First();
        }

        private class GuestSeatingPlan
        {
            public List<string> Guests { get; set; }
            public int OverallHappiness { get; set; }
        }

        private class NeighbourBasedHappinessMap : Dictionary<string, Dictionary<string, int>>
        {
            public static NeighbourBasedHappinessMap ParseMultiple(string input)
            {
                var map = new NeighbourBasedHappinessMap();

                foreach (var line in input.Trim().Split(Environment.NewLine))
                {
                    var parts = line.TrimEnd('.').Split(' ');
                    var guestName = parts[0];
                    var neighborName = parts[10];
                    var happinessImpact = Convert.ToInt32(parts[3]);
                    if (parts[2] == "lose")
                    {
                        happinessImpact = -happinessImpact;
                    }

                    if (!map.TryGetValue(guestName, out var guestPref))
                    {
                        guestPref = new Dictionary<string, int>();
                    }

                    guestPref[neighborName] = happinessImpact;
                    map[guestName] = guestPref;
                }

                return map;
            }

        }
        private const string InputData =
@"Alice would lose 2 happiness units by sitting next to Bob.
Alice would lose 62 happiness units by sitting next to Carol.
Alice would gain 65 happiness units by sitting next to David.
Alice would gain 21 happiness units by sitting next to Eric.
Alice would lose 81 happiness units by sitting next to Frank.
Alice would lose 4 happiness units by sitting next to George.
Alice would lose 80 happiness units by sitting next to Mallory.
Bob would gain 93 happiness units by sitting next to Alice.
Bob would gain 19 happiness units by sitting next to Carol.
Bob would gain 5 happiness units by sitting next to David.
Bob would gain 49 happiness units by sitting next to Eric.
Bob would gain 68 happiness units by sitting next to Frank.
Bob would gain 23 happiness units by sitting next to George.
Bob would gain 29 happiness units by sitting next to Mallory.
Carol would lose 54 happiness units by sitting next to Alice.
Carol would lose 70 happiness units by sitting next to Bob.
Carol would lose 37 happiness units by sitting next to David.
Carol would lose 46 happiness units by sitting next to Eric.
Carol would gain 33 happiness units by sitting next to Frank.
Carol would lose 35 happiness units by sitting next to George.
Carol would gain 10 happiness units by sitting next to Mallory.
David would gain 43 happiness units by sitting next to Alice.
David would lose 96 happiness units by sitting next to Bob.
David would lose 53 happiness units by sitting next to Carol.
David would lose 30 happiness units by sitting next to Eric.
David would lose 12 happiness units by sitting next to Frank.
David would gain 75 happiness units by sitting next to George.
David would lose 20 happiness units by sitting next to Mallory.
Eric would gain 8 happiness units by sitting next to Alice.
Eric would lose 89 happiness units by sitting next to Bob.
Eric would lose 69 happiness units by sitting next to Carol.
Eric would lose 34 happiness units by sitting next to David.
Eric would gain 95 happiness units by sitting next to Frank.
Eric would gain 34 happiness units by sitting next to George.
Eric would lose 99 happiness units by sitting next to Mallory.
Frank would lose 97 happiness units by sitting next to Alice.
Frank would gain 6 happiness units by sitting next to Bob.
Frank would lose 9 happiness units by sitting next to Carol.
Frank would gain 56 happiness units by sitting next to David.
Frank would lose 17 happiness units by sitting next to Eric.
Frank would gain 18 happiness units by sitting next to George.
Frank would lose 56 happiness units by sitting next to Mallory.
George would gain 45 happiness units by sitting next to Alice.
George would gain 76 happiness units by sitting next to Bob.
George would gain 63 happiness units by sitting next to Carol.
George would gain 54 happiness units by sitting next to David.
George would gain 54 happiness units by sitting next to Eric.
George would gain 30 happiness units by sitting next to Frank.
George would gain 7 happiness units by sitting next to Mallory.
Mallory would gain 31 happiness units by sitting next to Alice.
Mallory would lose 32 happiness units by sitting next to Bob.
Mallory would gain 95 happiness units by sitting next to Carol.
Mallory would gain 91 happiness units by sitting next to David.
Mallory would lose 66 happiness units by sitting next to Eric.
Mallory would lose 75 happiness units by sitting next to Frank.
Mallory would lose 99 happiness units by sitting next to George.
";
    }
}