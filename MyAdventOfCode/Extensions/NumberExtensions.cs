namespace MyAdventOfCode.Extensions;

public static class NumberExtensions
{
    public static bool Between(this long value, long min, long max, bool inclusive = true)
        => inclusive ? value >= min && value <= max : value > min && value < max;
}