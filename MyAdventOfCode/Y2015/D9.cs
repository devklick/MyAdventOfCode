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

        private class RoutePlanner
        {
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

            public Route GetShortestRoute()
            {
                string[] shortestTrip = null;
                int shortestDistance = 0;
                foreach (var trip in _allLocations.GetPermutations())
                {
                    var tripDistance = 0;
                    for (var i = 0; i < trip.Length; i++)
                    {
                        var iNext = i + 1;
                        if (iNext < trip.Length && _map.TryGetValue((trip[i], trip[iNext]), out var a2b))
                        {
                            tripDistance += a2b.Distance;
                        }
                    }
                    if (tripDistance < shortestDistance || shortestDistance == 0)
                    {
                        shortestDistance = tripDistance;
                        shortestTrip = trip;
                    }
                }


                return new Route
                {
                    Stops = shortestTrip.ToList(),
                    Distance = shortestDistance
                };
            }
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

    /// <summary>
    /// Lifted straight from <see href="https://stackoverflow.com/a/58826787/6236042"/>
    /// </summary>
    public static class PermutationExtension
    {
        public static IEnumerable<T[]> GetPermutations<T>(this IEnumerable<T> source)
        {
            var sourceArray = source.ToArray();
            var results = new List<T[]>();
            Permute(sourceArray, 0, sourceArray.Length - 1, results);
            return results;
        }

        private static void Swap<T>(ref T a, ref T b)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }

        private static void Permute<T>(T[] elements, int recursionDepth, int maxDepth, ICollection<T[]> results)
        {
            if (recursionDepth == maxDepth)
            {
                results.Add(elements.ToArray());
                return;
            }

            for (var i = recursionDepth; i <= maxDepth; i++)
            {
                Swap(ref elements[recursionDepth], ref elements[i]);
                Permute(elements, recursionDepth + 1, maxDepth, results);
                Swap(ref elements[recursionDepth], ref elements[i]);
            }
        }
    }
}