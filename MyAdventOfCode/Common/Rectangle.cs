using System;
using System.Collections.Generic;
using System.IO.Compression;

namespace MyAdventOfCode.Common;

public class Rectangle
{
    public Vector From { get; }
    public Vector To { get; }
    public int Width => Math.Abs(To.X - From.X);
    public int Height => Math.Abs(To.Y - From.Y);
    public int MinX => Math.Min(From.X, To.X);
    public int MinY => Math.Min(From.Y, To.Y);
    public int MaxX => Math.Max(From.X, To.X);
    public int MaxY => Math.Max(From.Y, To.Y);

    public Rectangle(int fromX, int fromY, int toX, int toY)
    {
        From = new Vector(fromX, fromY);
        To = new Vector(toX, toY);
    }

    public Rectangle(Vector from, Vector to)
    {
        From = from;
        To = to;
    }
    public Rectangle(RCVector from, RCVector to)
    {
        From = new Vector(from.Column, from.Row);
        To = new Vector(to.Column, to.Row);
    }


    public List<Vector> GetBorderPositions()
    {
        var positions = new List<Vector>();

        var minX = MinX - 1;
        var maxX = MaxX + 1;
        var minY = MinY - 1;
        var maxY = MaxY + 1;

        for (var x = minX; x <= maxX; x++)
        {
            for (var y = minY; y <= maxY; y++)
            {
                if (x == minX || x == maxX || y == minY || y == maxY)
                {
                    positions.Add(new Vector(x, y));
                }
            }
        }

        return positions;
    }

    public bool Contains(Vector position)
        => position.X >= MinX && position.Y <= MaxY
            && position.Y >= MinY && position.Y <= MaxY;

    public bool IsEdge(Vector position)
        => position.X == MinX || position.X == MaxX
        || position.Y == MinY || position.Y == MaxY;

    public bool IsCorner(Vector position)
        => (position.X == MinX || position.X == MaxX)
        && (position.Y == MinY || position.Y == MaxY);
}