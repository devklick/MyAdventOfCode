using System;
using System.Collections.Generic;
using System.Linq;

namespace MyAdventOfCode.Y2022.D12.Cheat;

public record Cell(int X, int Y, char Value);

public static class BreadthFirstSearch
{
    public static List<Cell> Search(HeightMap map, Cell start, Cell end)
    {
        var frontier = new Queue<Cell>();
        frontier.Enqueue(start);

        var cameFrom = new Dictionary<Cell, Cell?>
        {
            [start] = null
        };

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            if (current == end)
            {
                break;
            }

            foreach (var next in map.GetNeighbors(current))
            {
                if (!cameFrom.ContainsKey(next))
                {
                    frontier.Enqueue(next);
                    cameFrom[next] = current;
                }
            }
        }

        return BuildPath(start, end, cameFrom);
    }

    private static List<Cell> BuildPath(Cell start, Cell end, IReadOnlyDictionary<Cell, Cell?> cameFrom)
    {
        // End was not found, so no path can be built.
        if (!cameFrom.ContainsKey(end))
        {
            return new List<Cell>();
        }

        var pathCell = end;
        var path = new List<Cell>();

        while (pathCell != start)
        {
            path.Add(pathCell);

            // cameFrom[map.Start] is the only null value. Since we can't access it in this loop, we can suppress null here. 
            pathCell = cameFrom[pathCell]!;
        }
        path.Reverse();

        return path;
    }
}
public class HeightMap
{
    private const char StartValue = 'S';
    private const char EndValue = 'E';
    private const char SecondValue = 'a';
    private const char SecondToLastValue = 'z';

    public readonly int Columns;
    public readonly int Rows;

    public readonly Cell[,] Grid;
    public readonly Cell Start;
    public readonly Cell End;

    private readonly Dictionary<Cell, List<Cell>> _neighbors = new();

    public HeightMap(string[] lines)
    {
        Columns = lines[0].Length;
        Rows = lines.Length;

        Grid = new Cell[Rows, Columns];

        for (var row = 0; row < Rows; ++row)
        {
            for (var col = 0; col < Columns; ++col)
            {
                var cell = new Cell(col, row, lines[row][col]);

                Grid[row, col] = cell;

                if (cell.Value == StartValue)
                {
                    Start = cell;
                }
                else if (cell.Value == EndValue)
                {
                    End = cell;
                }
            }
        }

        if (Start is null)
        {
            throw new FormatException("Could not find Start position in map representation");
        }

        if (End is null)
        {
            throw new FormatException("Could not find End position in map representation");
        }

        for (var row = 0; row < Rows; ++row)
        {
            for (var col = 0; col < Columns; ++col)
            {
                var cell = Grid[row, col];

                var leftNeighbor = GetCellIfInBounds(row, col - 1);
                var rightNeighbor = GetCellIfInBounds(row, col + 1);
                var upNeighbor = GetCellIfInBounds(row - 1, col);
                var downNeighbor = GetCellIfInBounds(row + 1, col);

                var neighbors = new List<Cell?>
                {
                    leftNeighbor, rightNeighbor, upNeighbor, downNeighbor,
                };

                _neighbors[cell] = neighbors
                    .Where(neighbor => neighbor is not null && IsValidNeighbor(cell, neighbor))
                    .Select(neighbor => neighbor!)
                    .ToList();
            }
        }
    }

    public List<Cell> GetNeighbors(Cell cell) => _neighbors[cell];

    public Cell this[int x, int y] => Grid[y, x]; // Note: Grid[row, col] = Grid[y, x].

    private Cell? GetCellIfInBounds(int row, int col) =>
        row >= 0 && row < Rows && col >= 0 && col < Columns
            ? Grid[row, col]
            : null;

    private static bool IsValidNeighbor(Cell current, Cell neighbor)
    {
        if (current.Value == StartValue)
        {
            return neighbor.Value == SecondValue;
        }

        if (neighbor.Value == EndValue)
        {
            return current.Value == SecondToLastValue;
        }

        return neighbor.Value == StartValue || neighbor.Value - current.Value <= 1;
    }
}