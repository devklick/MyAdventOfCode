using System;
using System.Collections.Generic;

namespace MyAdventOfCode.Extensions;

public static class StringExtensions
{
    public static IEnumerable<(int i, string value)> SplitInParts(this string s, int partLength)
    {
        if (s == null)
            throw new ArgumentNullException(nameof(s));
        if (partLength <= 0)
            throw new ArgumentException("Part length has to be positive.", nameof(partLength));

        var iPart = 0;
        for (var i = 0; i < s.Length; i += partLength, iPart++)
            yield return (iPart, s.Substring(i, Math.Min(partLength, s.Length - i)));
    }

}