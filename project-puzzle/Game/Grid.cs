using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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

    public void CheckConnections()
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                cells[x, y].Disconnect();

        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                if (cells[x, y].State == CellState.StartBlockEmpty)
                {
                    Cell self = cells[x, y];
                    switch (cells[x, y].Rotation)
                    {
                        case RotationState.Up:
                            Cell topCell = cells[x, y - 1];

                            if (topCell.State == CellState.StartBlockEmpty && topCell.Rotation == RotationState.Down)
                            {
                                topCell.Connect();
                                self.Connect();
                            }
                            else if (topCell.State == CellState.TwoWayPathBlockEmpty && (topCell.Rotation == RotationState.Up || topCell.Rotation == RotationState.Down))
                            {
                                topCell.Connect();
                                self.Connect();
                            }

                            break;
                        case RotationState.Down:
                            break;
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