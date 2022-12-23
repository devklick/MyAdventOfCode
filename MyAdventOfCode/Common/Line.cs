using System;
using System.Collections.Generic;

namespace MyAdventOfCode.Common;

public class Line
{
    public Vector From { get; }
    public Vector To { get; }

    public Line(Vector from, Vector to)
    {
        From = from;
        To = to;
    }

    /// <summary>
    /// See <see href="https://stackoverflow.com/a/11683720/6236042"/>
    /// </summary>
    public List<Vector> GetPoints()
    {
        int x = From.X, y = From.Y;

        int w = To.X - From.X;
        int h = To.Y - From.Y;

        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
        if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
        if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
        if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;

        int longest = Math.Abs(w);
        int shortest = Math.Abs(h);

        if (!(longest > shortest))
        {
            longest = Math.Abs(h);
            shortest = Math.Abs(w);
            if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
            dx2 = 0;
        }


        var points = new List<Vector>();
        int numerator = longest >> 1;
        for (int i = 0; i <= longest; i++)
        {
            points.Add(new Vector(x, y));
            numerator += shortest;
            if (!(numerator < longest))
            {
                numerator -= longest;
                x += dx1;
                y += dy1;
            }
            else
            {
                x += dx2;
                y += dy2;
            }
        }
        return points;
    }
}