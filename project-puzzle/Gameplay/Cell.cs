// List of rectangles for the tileset
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Gameplay;

public static class CellTexture
{
    public const int CellSize = 32;

    public static readonly Rectangle Empty = new(0, 64, 32, 32);
    public static readonly Rectangle Invisible = new(0, 96, 32, 32);
    public static readonly Rectangle Blocker = new(32, 0, 32, 32);
    public static readonly Rectangle Symbol1 = new(64, 0, 32, 32);
    public static readonly Rectangle Symbol2 = new(96, 0, 32, 32);
    public static readonly Rectangle Symbol3 = new(128, 0, 32, 32);

    public static readonly Rectangle DecorationLine = new(192, 64, 32, 32);
    public static readonly Rectangle DecorationCorner = new(192, 32, 32, 32);
    public static readonly Rectangle Select = new(160, 32, 32, 32);

}

public enum CellType
{
    Symbol1,
    Symbol2,
    Symbol3
}

public enum CellState
{
    Empty,
    Placeholder,
    Invisible,
    Blocker,
    Symbol1,
    Symbol2,
    Symbol3
}

public static class CellHelpers
{
    // Symbol1 beats symbol2, symbol2 beats symbol3, symbol3 beats symbol1
    public static bool Beats(this CellState a, CellState b)
    {
        return (a == CellState.Symbol1 && b == CellState.Symbol2) ||
               (a == CellState.Symbol2 && b == CellState.Symbol3) ||
               (a == CellState.Symbol3 && b == CellState.Symbol1);
    }
}

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public class Cell
{
    public bool IsClearing { get; set; }

    public Cell(CellState state = CellState.Empty)
    {
        State = state;
    }

    private Rectangle UpdateSourceRect()
    {
        return State switch
        {
            CellState.Empty => CellTexture.Empty,
            CellState.Invisible => CellTexture.Invisible,
            CellState.Blocker => CellTexture.Blocker,
            CellState.Symbol1 => CellTexture.Symbol1,
            CellState.Symbol2 => CellTexture.Symbol2,
            CellState.Symbol3 => CellTexture.Symbol3,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private CellState _state;
    public CellState State
    {
        get => _state;
        set
        {
            _state = value;
            SourceRect = UpdateSourceRect();
        }
    }

    public int X { get; set; }
    public int Y { get; set; }

    public Rectangle SourceRect;

    public void Clear()
    {
        State = CellState.Empty;
    }

    public List<(Direction, CellState)> CheckNeighborStates(Cell[,] grid, int x, int y)
    {
        List<(Direction, CellState)> results = [];

        if (y > 0 && State.Beats(grid[x, y - 1].State)) results.Add((Direction.Up, grid[x, y - 1].State));
        if (y < grid.GetLength(1) - 1 && State.Beats(grid[x, y + 1].State)) results.Add((Direction.Down, grid[x, y + 1].State));
        if (x > 0 && State.Beats(grid[x - 1, y].State)) results.Add((Direction.Left, grid[x - 1, y].State));
        if (x < grid.GetLength(0) - 1 && State.Beats(grid[x + 1, y].State)) results.Add((Direction.Right, grid[x + 1, y].State));

        return results;
    }
}

