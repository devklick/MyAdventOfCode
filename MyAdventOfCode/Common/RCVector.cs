using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using MyAdventOfCode.Common.Attributes;

namespace MyAdventOfCode.Common;
public class RCVector : IEquatable<RCVector>
{
    public static readonly RCVector Default = new(0, 0);

    [Alias("N", "U")]
    public static readonly NorthRCVector North = new();

    [Alias("E", "R")]
    public static readonly EastRCVector East = new();

    [Alias("S", "D")]
    public static readonly SouthRCVector South = new();

    [Alias("W", "L")]
    public static readonly WestRCVector West = new();

    [Alias("NE", "UR")]
    public static readonly NorthEastRCVector NorthEast = new();

    [Alias("SE", "DR")]
    public static readonly SouthEastRCVector SouthEast = new();

    [Alias("SW", "DL")]
    public static readonly SouthWestRCVector SouthWest = new();

    [Alias("NW", "UL")]
    public static readonly NorthWestRCVector NorthWest = new();

    public static readonly RCVector[] Cardinals = new RCVector[] { North, East, South, West };
    public static readonly RCVector[] Ordinals = new RCVector[] { NorthEast, SouthEast, SouthWest, NorthWest };
    public static readonly RCVector[] All = new RCVector[] { North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest };

    private static readonly List<(string[] Aliases, RCVector Direction)> AliasMap;

    static RCVector()
    {
        AliasMap = new List<(string[] Aliases, RCVector Direction)>();
        var fields = typeof(RCVector).GetFields(BindingFlags.Static | BindingFlags.Public);

        foreach (var field in fields)
        {
            var aliasAttr = field.GetCustomAttribute<AliasAttribute>();
            if (aliasAttr == null) continue;
            var value = (RCVector)field.GetValue(null);
            AliasMap.Add((aliasAttr.Aliases, value));
        }
    }

    public int Row { get; }
    public int Column { get; }

    public RCVector(int row, int column)
    {
        Row = row;
        Column = column;
    }

    public static RCVector Parse(string input)
    {
        var candidates = AliasMap.Where(a => a.Aliases.Contains(input));
        var count = candidates.Count();

        return count == 1
            ? candidates.First().Direction
            : throw new InvalidOperationException(
                $"Unable to parse input {input} to {nameof(RCVector)}. "
                + "Expected to find exactly one parse candidate but found {count}");
    }

    public static IEnumerable<RCVector> GetNeighbors(RCVector position, RCVector[] directions)
    {
        foreach (var direction in directions)
        {
            yield return position.Move(direction);
        }
    }

    public RCVector Move(RCVector direction, int by = 1) => new(Row + direction.Row * by, Column + direction.Column * by);

    public int DistanceFrom(RCVector other, RCVector direction)
    {
        // TODO: Probably a better way of doing this. Might try and figure it out...
        return direction switch
        {
            NorthRCVector => Row - other.Row,
            EastRCVector => other.Column - Column,
            SouthRCVector => other.Row - Row,
            WestRCVector => Column - other.Column,
            _ => throw new NotImplementedException("Direction not yet supported")
        };
    }

    public bool IsNextTo(RCVector other, RCVector[] directions = null)
    {
        directions ??= All;
        if (!directions.Any()) return false;

        foreach (var direction in directions)
        {
            if (Move(direction) == other) return true;
        }
        return false;
    }

    public bool Equals(RCVector other)
        => other != null && other.Column == Column && other.Row == Row;

    public override bool Equals(object obj)
        => Equals(obj as RCVector);

    public static bool operator ==(RCVector a, RCVector b)
    {
        if (ReferenceEquals(a, b))
            return true;

        if (a is null)
            return false;

        if (b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(RCVector a, RCVector b)
        => !(a == b);

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Row);
        hashCode.Add(Column);
        return hashCode.ToHashCode();
    }

    public bool IsCardinallyAlignedWith(RCVector other)
        => Row == other.Row || Column == other.Column;

    public override string ToString()
        => $"R{Row},C{Column}";
}

public class NorthRCVector : RCVector
{
    public NorthRCVector() : base(-1, 0) { }
}
public class EastRCVector : RCVector
{
    public EastRCVector() : base(0, 1) { }
}
public class SouthRCVector : RCVector
{
    public SouthRCVector() : base(1, 0) { }
}
public class WestRCVector : RCVector
{
    public WestRCVector() : base(0, -1) { }
}
public class NorthEastRCVector : RCVector
{
    public NorthEastRCVector() : base(-1, 1) { }
}
public class SouthEastRCVector : RCVector
{
    public SouthEastRCVector() : base(1, 1) { }
}
public class SouthWestRCVector : RCVector
{
    public SouthWestRCVector() : base(1, -1) { }
}
public class NorthWestRCVector : RCVector
{
    public NorthWestRCVector() : base(-1, -1) { }
}
