using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public enum GridPhase
{
    Playing,
    Gravity,
    Clearing,
    WaitingAfterClear,
    GameOver
}

public class Grid : IGameObject
{
    public int Width = 6;
    public int Height = 12;

    public int CellSize = 32;

    private readonly int _screenWidth;
    private readonly int _screenHeight;

    public int OffsetX => (_screenWidth - Width * CellSize) / 2;
    public int OffsetY => (_screenHeight - Height * CellSize) / 2;

    private readonly Texture2D _texture;

    private GridPhase _phase = GridPhase.Playing;
    private double _phaseTimer = 0;
    private const double GravityStepDelay = 0.05;
    private const double ClearDelay = 0.4;

    public bool IsAnimating => _phase != GridPhase.Playing && _phase != GridPhase.GameOver;
    public bool IsGameOver => _phase == GridPhase.GameOver;

    public event Action OnGameOver;

    private readonly Cell[,] cells;

    public Grid(Texture2D texture, int screenWidth, int screenHeight)
    {
        _texture = texture;

        _screenWidth = screenWidth;
        _screenHeight = screenHeight;

        cells = new Cell[Width, Height];
        Reset();
    }

    public bool IsCellEmpty(int x, int y)
    {
        return cells[x, y].State == CellState.Empty;
    }

    public CellState GetCellState(int x, int y)
    {
        return cells[x, y].State;
    }

    public void SetCell(int x, int y, Cell cell)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return;
        cell.X = x;
        cell.Y = y;
        cells[x, y] = cell;
    }

    public bool IsValidPosition(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return false;
        return IsCellEmpty(x, y);
    }

    public bool IsValidPosition(int x, int y, Cell[,] matrix)
    {
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                if (matrix[i, j].State == CellState.Empty) continue;
                if (!IsValidPosition(x + i, y + j)) return false;
            }
        }
        return true;
    }

    public void PlacePiece(int x, int y, Cell[,] matrix)
    {
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                if (matrix[i, j].State != CellState.Empty)
                {
                    Cell src = matrix[i, j];
                    SetCell(x + i, y + j, new Cell(src.State));
                }
            }
        }
        _phase = GridPhase.Gravity;
        _phaseTimer = 0;
    }

    private bool ApplyGravityOneStep()
    {
        bool moved = false;
        for (int x = 0; x < Width; x++)
        {
            for (int y = Height - 2; y >= 0; y--)
            {
                if (cells[x, y].State == CellState.Empty || cells[x, y].IsClearing) continue;
                if (cells[x, y + 1].State == CellState.Empty)
                {
                    cells[x, y + 1] = cells[x, y];
                    cells[x, y] = new Cell();
                    moved = true;
                }
            }
        }
        return moved;
    }

    private bool MarkMatches()
    {
        bool found = false;

        for (int x = 0; x < Width; x++)
        {
            for (int y = Height - 1; y >= 0; y--)
            {
                Cell cell = cells[x, y];
                if (cell.State == CellState.Empty || cell.State == CellState.Placeholder || cell.State == CellState.Invisible || cell.IsClearing) continue;

                var results = cell.CheckNeighborStates(cells, x, y);
                if (results.Count == 0) continue;

                bool cleared = false;

                foreach (var (direction, state) in results)
                {
                    List<Cell> matched = [];

                    if (direction == Direction.Up)
                    {
                        for (int i = y - 1; i >= 0; i--)
                        {
                            if (cells[x, i].State == state && !cells[x, i].IsClearing) matched.Add(cells[x, i]);
                            else break;
                        }
                    }
                    else if (direction == Direction.Down)
                    {
                        for (int i = y + 1; i < Height; i++)
                        {
                            if (cells[x, i].State == state && !cells[x, i].IsClearing) matched.Add(cells[x, i]);
                            else break;
                        }
                    }
                    else if (direction == Direction.Left)
                    {
                        for (int i = x - 1; i >= 0; i--)
                        {
                            if (cells[i, y].State == state && !cells[i, y].IsClearing) matched.Add(cells[i, y]);
                            else break;
                        }
                    }
                    else if (direction == Direction.Right)
                    {
                        for (int i = x + 1; i < Width; i++)
                        {
                            if (cells[i, y].State == state && !cells[i, y].IsClearing) matched.Add(cells[i, y]);
                            else break;
                        }
                    }

                    if (matched.Count >= 3)
                    {
                        foreach (Cell c in matched) c.IsClearing = true;
                        cleared = true;
                        found = true;
                    }
                }

                if (cleared) cell.IsClearing = true;
            }
        }

        return found;
    }

    private void RemoveMarkedCells()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (cells[x, y].IsClearing)
                {
                    cells[x, y] = new Cell();
                }
            }
        }
    }

    public void Update(GameTime gameTime)
    {
        if (_phase == GridPhase.Playing) return;

        _phaseTimer += gameTime.ElapsedGameTime.TotalSeconds;

        switch (_phase)
        {
            case GridPhase.Gravity:
                if (_phaseTimer >= GravityStepDelay)
                {
                    _phaseTimer = 0;
                    bool moved = ApplyGravityOneStep();
                    SoundManager.Play(Sounds.Fall);
                    if (!moved)
                        _phase = GridPhase.Clearing;
                }
                break;

            case GridPhase.Clearing:
                bool hadMatches = MarkMatches();
                if (hadMatches)
                {
                    SoundManager.Play(Sounds.ClearMatch);
                    _phase = GridPhase.WaitingAfterClear;
                    _phaseTimer = 0;
                }
                else if (CheckGameOver())
                {
                    _phase = GridPhase.GameOver;
                    OnGameOver?.Invoke();
                }
                else
                {
                    _phase = GridPhase.Playing;
                }
                break;

            case GridPhase.WaitingAfterClear:
                if (_phaseTimer >= ClearDelay)
                {
                    RemoveMarkedCells();
                    _phase = GridPhase.Gravity;
                    _phaseTimer = 0;
                }
                break;
        }
    }

    private bool CheckGameOver()
    {
        int threshold = 1;
        for (int x = 0; x < Width; x++)
        {
            if (cells[x, threshold].State != CellState.Empty)
                return true;
        }
        return false;
    }

    public void Reset()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                cells[x, y] = new Cell();
            }
        }

        cells[0, Height - 1] = new Cell(CellState.Invisible);
        cells[0, Height - 2] = new Cell(CellState.Invisible);
        cells[1, Height - 1] = new Cell(CellState.Invisible);
        cells[Width - 1, Height - 1] = new Cell(CellState.Invisible);
        cells[Width - 1, Height - 2] = new Cell(CellState.Invisible);
        cells[Width - 2, Height - 1] = new Cell(CellState.Invisible);

        _phase = GridPhase.Playing;
        _phaseTimer = 0;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        // Draw background
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (cells[x, y].State == CellState.Invisible) continue;

                Vector2 origin = new(CellSize / 2, CellSize / 2);
                spriteBatch.Draw(_texture, new Rectangle(OffsetX + x * CellSize + CellSize / 2, OffsetY + y * CellSize + CellSize / 2, CellSize, CellSize), CellTexture.Empty, Color.White, 0f, origin, SpriteEffects.None, 0f);
            }
        }

        // Draw cells

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (cells[x, y].State == CellState.Empty) continue;

                Cell cell = cells[x, y];
                Vector2 origin = new(CellSize / 2, CellSize / 2);
                Color tint = cell.IsClearing ? Color.White * 0.4f : Color.White;
                spriteBatch.Draw(_texture, new Rectangle(OffsetX + x * CellSize + CellSize / 2, OffsetY + y * CellSize + CellSize / 2, CellSize, CellSize), cell.SourceRect, tint, 0f, origin, SpriteEffects.None, 0f);
            }
        }
    }
}