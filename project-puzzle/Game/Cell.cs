// List of rectangles for the tileset
using System;
using Microsoft.Xna.Framework;

public static class CellTexture
{
    public const int CellSize = 32;

    public static readonly Rectangle Empty = new(0, 0, 32, 32);
    public static readonly Rectangle Placeholder = new(32, 0, 32, 32);
    public static readonly Rectangle StartBlockEmpty = new(64, 0, 32, 32);
    public static readonly Rectangle StartBlockConnected = new(64, 32, 32, 32);
    public static readonly Rectangle TwoWayPathBlockEmpty = new(96, 0, 32, 32);
    public static readonly Rectangle TwoWayPathBlockConnected = new(96, 32, 32, 32);
    public static readonly Rectangle ThreeWayPathBlockEmpty = new(64, 64, 32, 32);
    public static readonly Rectangle ThreeWayPathBlockConnected = new(96, 64, 32, 32);
    public static readonly Rectangle FourWayPathBlockEmpty = new(64, 96, 32, 32);
    public static readonly Rectangle FourWayPathBlockConnected = new(96, 96, 32, 32);

}

public enum CellState
{
    Empty,
    Placeholder,
    StartBlockEmpty,
    StartBlockConnected,
    TwoWayPathBlockEmpty,
    TwoWayPathBlockConnected,
    ThreeWayPathBlockEmpty,
    ThreeWayPathBlockConnected,
    FourWayPathBlockEmpty,
    FourWayPathBlockConnected
}

public static class CellHelpers
{
    public static CellState ToConnected(CellState state) => state switch
    {
        CellState.StartBlockEmpty => CellState.StartBlockConnected,
        CellState.TwoWayPathBlockEmpty => CellState.TwoWayPathBlockConnected,
        CellState.ThreeWayPathBlockEmpty => CellState.ThreeWayPathBlockConnected,
        CellState.FourWayPathBlockEmpty => CellState.FourWayPathBlockConnected,
        _ => state
    };

    public static CellState ToDisconnected(CellState state) => state switch
    {
        CellState.StartBlockConnected => CellState.StartBlockEmpty,
        CellState.TwoWayPathBlockConnected => CellState.TwoWayPathBlockEmpty,
        CellState.ThreeWayPathBlockConnected => CellState.ThreeWayPathBlockEmpty,
        CellState.FourWayPathBlockConnected => CellState.FourWayPathBlockEmpty,
        _ => state
    };

    public static bool IsConnected(CellState state) =>
        state is CellState.StartBlockConnected or CellState.TwoWayPathBlockConnected
            or CellState.ThreeWayPathBlockConnected or CellState.FourWayPathBlockConnected;
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
            CellState.StartBlockEmpty => CellTexture.StartBlockEmpty,
            CellState.StartBlockConnected => CellTexture.StartBlockConnected,
            CellState.TwoWayPathBlockEmpty => CellTexture.TwoWayPathBlockEmpty,
            CellState.TwoWayPathBlockConnected => CellTexture.TwoWayPathBlockConnected,
            CellState.ThreeWayPathBlockEmpty => CellTexture.ThreeWayPathBlockEmpty,
            CellState.ThreeWayPathBlockConnected => CellTexture.ThreeWayPathBlockConnected,
            CellState.FourWayPathBlockEmpty => CellTexture.FourWayPathBlockEmpty,
            CellState.FourWayPathBlockConnected => CellTexture.FourWayPathBlockConnected,
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

    public void Connect()
    {
        State = CellHelpers.ToConnected(State);
    }

    public void Disconnect()
    {
        State = CellHelpers.ToDisconnected(State);
    }

    public bool IsConnected() => CellHelpers.IsConnected(State);

    public Rectangle SourceRect;
}

