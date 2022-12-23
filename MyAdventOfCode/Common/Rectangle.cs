namespace MyAdventOfCode.Common;

public class Rectangle
{
    public Vector From { get; }
    public Vector To { get; }

    public Rectangle(int fromX, int fromY, int toX, int toY)
    {
        From = new Vector(fromX, fromY);
        To = new Vector(toX, toY);
    }
}