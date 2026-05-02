using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

public class Grid : IGameObject
{
    public int Width = 15;
    public int Height = 15;

    public int CellSize = 32;

    public int OffsetX = 200;
    public int OffsetY = 100;

    private readonly Texture2D _texture;

    private readonly Cell[,] cells;

    public Grid(Texture2D texture)
    {
        _texture = texture;

        cells = new Cell[Width, Height];
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                cells[x, y] = new Cell();
            }
        }
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
                    SetCell(x + i, y + j, new Cell(src.State, src.Rotation));
                }
            }
        }
        ApplyGravity();
        CheckConnections();
    }

    public void ApplyGravity()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = Height - 2; y >= 0; y--)
            {
                if (cells[x, y].State == CellState.Empty) continue;

                int dropY = y;
                while (dropY < Height - 1 && cells[x, dropY + 1].State == CellState.Empty)
                {
                    dropY++;
                }

                if (dropY != y)
                {
                    cells[x, dropY] = cells[x, y];
                    cells[x, y] = new Cell();
                }
            }
        }
    }

    private static (int dx, int dy)[] GetOpenings(Cell cell)
    {
        switch (cell.State)
        {
            case CellState.StartBlockEmpty:
            case CellState.StartBlockConnected:
                return cell.Rotation switch
                {
                    RotationState.Up => [(0, -1)],
                    RotationState.Down => [(0, 1)],
                    RotationState.Left => [(-1, 0)],
                    RotationState.Right => [(1, 0)],
                    _ => []
                };
            case CellState.TwoWayPathBlockEmpty:
            case CellState.TwoWayPathBlockConnected:
                return cell.Rotation == RotationState.Up || cell.Rotation == RotationState.Down
                    ? [(0, -1), (0, 1)]
                    : [(-1, 0), (1, 0)];
            default:
                return [];
        }
    }

    private static bool IsStart(Cell c) =>
        c.State is CellState.StartBlockEmpty or CellState.StartBlockConnected;

    public void CheckConnections()
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                cells[x, y].Disconnect();

        for (int sx = 0; sx < Width; sx++)
        {
            for (int sy = 0; sy < Height; sy++)
            {
                Cell start = cells[sx, sy];
                if (!IsStart(start)) continue;

                var openings = GetOpenings(start);
                if (openings.Length == 0) continue;

                List<Cell> path = [start];
                int prevX = sx, prevY = sy;
                int cx = sx + openings[0].dx, cy = sy + openings[0].dy;
                bool reachedStart = false;

                while (cx >= 0 && cx < Width && cy >= 0 && cy < Height)
                {
                    Cell next = cells[cx, cy];
                    var nextOpenings = GetOpenings(next);

                    bool connectsBack = false;
                    foreach (var (ndx, ndy) in nextOpenings)
                    {
                        if (cx + ndx == prevX && cy + ndy == prevY) { connectsBack = true; break; }
                    }
                    if (!connectsBack) break;

                    path.Add(next);

                    if (IsStart(next)) { reachedStart = true; break; }

                    int odx = 0, ody = 0;
                    bool found = false;
                    foreach (var (ndx, ndy) in nextOpenings)
                    {
                        if (cx + ndx == prevX && cy + ndy == prevY) continue;
                        odx = ndx; ody = ndy; found = true; break;
                    }
                    if (!found) break;

                    prevX = cx; prevY = cy;
                    cx += odx; cy += ody;
                }

                if (reachedStart)
                {
                    foreach (Cell c in path) c.Connect();
                }
            }
        }
    }

    public void Update(GameTime gameTime)
    {
        // Update grid logic here if needed
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Cell cell = cells[x, y];
                int rotation = (int)cell.Rotation;
                Vector2 origin = new(CellSize / 2, CellSize / 2);
                spriteBatch.Draw(_texture, new Rectangle(OffsetX + x * CellSize + CellSize / 2, OffsetY + y * CellSize + CellSize / 2, CellSize, CellSize), cell.SourceRect, Color.White, rotation * (float)(Math.PI / 180), origin, SpriteEffects.None, 0f);
            }
        }
    }
}