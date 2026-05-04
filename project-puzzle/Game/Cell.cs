// List of rectangles for the tileset
using System;
using Microsoft.Xna.Framework;

public static class CellTexture
{
    public const int CellSize = 32;

    public static readonly Rectangle Empty = new(0, 0, 32, 32);
    public static readonly Rectangle Placeholder = new(32, 0, 32, 32);
    public static readonly Rectangle Symbol1 = new(64, 0, 32, 32);
    public static readonly Rectangle Symbol2 = new(96, 0, 32, 32);
    public static readonly Rectangle Symbol3 = new(96, 32, 32, 32);
}

public enum CellState
{
    Empty,
    Placeholder,
    Symbol1,
    Symbol2,
    Symbol3
}

public static class CellHelpers
{

}

public class Cell
{
    public Cell(CellState state = CellState.Empty)
    {
        State = state;
        SourceRect = UpdateSourceRect();
    }

    private Rectangle UpdateSourceRect()
    {
        return State switch
        {
            CellState.Empty => CellTexture.Empty,
            CellState.Placeholder => CellTexture.Placeholder,
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
}

