using System;
using System.Collections.Generic;
using System.Linq;
using MyAdventOfCode.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2015
{
    public class D9
    {
        private readonly ITestOutputHelper _output;
        public D9(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Part_1_Example()
        {
            const string testData = "London to Dublin = 464\nLondon to Belfast = 518\nDublin to Belfast = 141";
            var distances = A2BDistance.ParseMultiple(testData);
            var planner = new RoutePlanner(distances);
            var shortest = planner.GetShortestRoute();

            Assert.Equal(605, shortest.Distance);
            Assert.Equal("London -> Dublin -> Belfast = 605", shortest.ToString());
        }

        [Fact]
        public void Part_1()
        {
            var distances = A2BDistance.ParseMultiple(TestData);
            var planner = new RoutePlanner(distances);
            var shortest = planner.GetShortestRoute();

            _output.WriteLine(shortest.Distance);
            _output.WriteLine(shortest);
        }

        [Fact]
        public void Part_2()
        {
            var distances = A2BDistance.ParseMultiple(TestData);
            var planner = new RoutePlanner(distances);
            var longest = planner.GetLongestRoute();

            _output.WriteLine(longest.Distance);
            _output.WriteLine(longest);
        }

        private class RoutePlanner
        {
            public enum RouteType { Shortest, Longest };
            Dictionary<(string PointA, string PointB), A2BDistance> _map;
            private HashSet<string> _allLocations;
            public RoutePlanner(IEnumerable<A2BDistance> distances)
            {
                _map = new Dictionary<(string PointA, string PointB), A2BDistance>();
                _allLocations = new HashSet<string>();
                foreach (var distance in distances)
                {
                    _map.Add((distance.PointA, distance.PointB), distance);

                    var reverse = distance.Reverse();
                    _map.TryAdd((reverse.PointA, reverse.PointB), reverse);

                    _allLocations.Add(distance.PointA);
                    _allLocations.Add(distance.PointB);
                }
            }

            public Route GetRoute(RouteType routeType)
            {
                string[] chosenRoute = null;
                int chosenRouteDistance = 0;
                foreach (var route in _allLocations.GetPermutations())
                {
                    var routeDistance = 0;
                    for (var i = 0; i < route.Length; i++)
                    {
                        var iNext = i + 1;
                        if (iNext < route.Length && _map.TryGetValue((route[i], route[iNext]), out var a2b))
                        {
                            routeDistance += a2b.Distance;
                        }
                    }
                    if (routeType == RouteType.Shortest
                        && (routeDistance < chosenRouteDistance || chosenRouteDistance == 0))
                    {
                        chosenRouteDistance = routeDistance;
                        chosenRoute = route;
                    }
                    else if (routeType == RouteType.Longest
                        && (routeDistance > chosenRouteDistance || chosenRouteDistance == 0))
                    {
                        chosenRouteDistance = routeDistance;
                        chosenRoute = route;
                    }
                }

                return new Route
                {
                    Stops = chosenRoute.ToList(),
                    Distance = chosenRouteDistance
                };
            }

            public Route GetShortestRoute()
                => GetRoute(RouteType.Shortest);

            public Route GetLongestRoute()
                => GetRoute(RouteType.Longest);
        }

        private class Route
        {
            public List<string> Stops { get; set; }
            public int Distance { get; set; }

            public override string ToString()
            {
                var stops = Stops.Aggregate((from, to) => $"{from} -> {to}");
                return $"{stops} = {Distance}";
            }
        }
        private class A2BDistance
        {
            public string PointA { get; set; }
            public string PointB { get; set; }
            public int Distance { get; set; }

            public A2BDistance Reverse() => new A2BDistance
            {
                PointA = this.PointB,
                PointB = this.PointA,
                Distance = this.Distance
            };

            public static IEnumerable<A2BDistance> ParseMultiple(string template)
            {
                foreach (var line in template.Split(Environment.NewLine))
                {
                    yield return Parse(line);
                }
            }

            public static A2BDistance Parse(string template)
            {
                string[] parts = template.Split(' ');
                return new A2BDistance
                {
                    PointA = parts[0],
                    PointB = parts[2],
                    Distance = int.Parse(parts[4])
                };
            }

            public override string ToString() => $"{PointA} to {PointB} = {Distance}";
        }
        private string TestData =
@"Faerun to Tristram = 65
Faerun to Tambi = 129
Faerun to Norrath = 144
Faerun to Snowdin = 71
Faerun to Straylight = 137
Faerun to AlphaCentauri = 3
Faerun to Arbre = 149
Tristram to Tambi = 63
Tristram to Norrath = 4
Tristram to Snowdin = 105
Tristram to Straylight = 125
Tristram to AlphaCentauri = 55
Tristram to Arbre = 14
Tambi to Norrath = 68
Tambi to Snowdin = 52
Tambi to Straylight = 65
Tambi to AlphaCentauri = 22
Tambi to Arbre = 143
Norrath to Snowdin = 8
Norrath to Straylight = 23
Norrath to AlphaCentauri = 136
Norrath to Arbre = 115
Snowdin to Straylight = 101
Snowdin to AlphaCentauri = 84
Snowdin to Arbre = 96
Straylight to AlphaCentauri = 107
Straylight to Arbre = 14
AlphaCentauri to Arbre = 46";
    }
}