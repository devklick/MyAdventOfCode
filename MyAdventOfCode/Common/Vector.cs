using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using MyAdventOfCode.Common.Attributes;

namespace MyAdventOfCode.Common;

public class Vector : IEquatable<Vector>
{
    public static readonly Vector Default = new(0, 0);

    [Alias("N", "U")]
    public static readonly NorthVector North = new();

    [Alias("E", "R")]
    public static readonly EastVector East = new();

    [Alias("S", "D")]
    public static readonly SouthVector South = new();

    [Alias("W", "L")]
    public static readonly WestVector West = new();

    [Alias("NE", "UR")]
    public static readonly NorthEastVector NorthEast = new();

    [Alias("SE", "DR")]
    public static readonly SouthEastVector SouthEast = new();

    [Alias("SW", "DL")]
    public static readonly SouthWestVector SouthWest = new();

    [Alias("NW", "UL")]
    public static readonly NorthWestVector NorthWest = new();

    public static readonly Vector[] Cardinals = new Vector[] { North, East, South, West };
    public static readonly Vector[] Ordinals = new Vector[] { NorthEast, SouthEast, SouthWest, NorthWest };
    public static readonly Vector[] All = new Vector[] { North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest };
    private static readonly List<(string[] Aliases, Vector Direction)> AliasMap = GetAliasMap<Vector>();

    public int X { get; }
    public int Y { get; }

    public Vector(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static Vector Parse(string input)
    {
        var candidates = AliasMap.Where(a => a.Aliases.Contains(input));
        var count = candidates.Count();

        return count == 1
            ? candidates.First().Direction as Vector
            : throw new InvalidOperationException(
                $"Unable to parse input {input} to {nameof(Vector)}. "
                + "Expected to find exactly one parse candidate but found {count}");
    }

    public static IEnumerable<Vector> GetNeighbors(Vector position, Vector[] directions)
    {
        foreach (var direction in directions)
        {
            yield return position.Move(direction);
        }
    }

    public Vector Move(Vector direction, int by = 1) => new(X + direction.X * by, Y + direction.Y * by);

    public int DistanceFrom(Vector other, Vector direction)
    {
        // TODO: Probably a better way of doing this. Might try and figure it out...
        return direction switch
        {
            NorthVector => X - other.X,
            EastVector => other.Y - Y,
            SouthVector => other.X - X,
            WestVector => Y - other.Y,
            _ => throw new NotImplementedException("Direction not yet supported")
        };
    }

    public bool IsNextTo(Vector other, Vector[] directions = null)
    {
        directions ??= All;
        if (!directions.Any()) return false;

        foreach (var direction in directions)
        {
            if (Move(direction) == other) return true;
        }
        return false;
    }

    public bool Equals(Vector other)
        => other != null && other.X == X && other.Y == Y;

    public override bool Equals(object obj)
        => Equals(obj as Vector);

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(X);
        hashCode.Add(Y);
        return hashCode.ToHashCode();
    }

    public override string ToString()
        => $"X{X},Y{Y}";

    public Vector RotateAxes()
        => new(Y, X);

    protected static List<(string[] Aliases, TVector Direction)> GetAliasMap<TVector>() where TVector : Vector
    {
        var aliasMap = new List<(string[] Aliases, TVector Direction)>();
        var fields = typeof(TVector).GetFields(BindingFlags.Static | BindingFlags.Public);

        foreach (var field in fields)
        {
            var aliasAttr = field.GetCustomAttribute<AliasAttribute>();
            if (aliasAttr == null) continue;
            var value = (TVector)field.GetValue(null);
            aliasMap.Add((aliasAttr.Aliases, value));
        }
        return aliasMap;
    }
}

public class NorthVector : Vector
{
    public NorthVector() : base(0, -1) { }
}
public class EastVector : Vector
{
    public EastVector() : base(1, 0) { }
}
public class SouthVector : Vector
{
    public SouthVector() : base(0, 1) { }
}
public class WestVector : Vector
{
    public WestVector() : base(-1, 0) { }
}
public class NorthEastVector : Vector
{
    public NorthEastVector() : base(1, -1) { }
}
public class SouthEastVector : Vector
{
    public SouthEastVector() : base(1, 1) { }
}
public class SouthWestVector : Vector
{
    public SouthWestVector() : base(-1, 1) { }
}
public class NorthWestVector : Vector
{
    public NorthWestVector() : base(-1, -1) { }
}
