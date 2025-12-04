using System;
using System.Collections.Generic;
using System.Linq;

using MyAdventOfCode.Common.Attributes;

namespace MyAdventOfCode.Common;

public enum Direction
{
    [Alias("L")]
    Left = -1,

    [Alias("R")]
    Right = 1
}

public static class DirectionParser
{
    private static readonly Dictionary<string, Direction> DirMap =
        EnumHelper.GetEnumMembersAndAttributes<Direction, AliasAttribute>()
        .ToDictionary(e => e.Value.Aliases.First(), e => e.Key);

    public static Direction Parse(char raw)
        => Parse(raw.ToString());

    public static Direction Parse(string raw)
    {
        return !DirMap.TryGetValue(raw, out var direction)
            ? throw new ArgumentException($"No known direction associated with value {raw}", nameof(raw))
            : direction;
    }
}