
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public static class PieceShapes
{
    public static readonly Cell[][,] All =
    [
        new Cell[2, 2] { { new Cell(CellState.StartBlockEmpty, RotationState.Up), new Cell() }, { new Cell(), new Cell() } },
        new Cell[2, 2] { { new Cell(CellState.TwoWayPathBlockEmpty, RotationState.Up), new Cell() }, { new Cell(), new Cell() } },
        new Cell[2, 2] { { new Cell(CellState.TwoWayPathBlockEmpty), new Cell() }, { new Cell(CellState.TwoWayPathBlockEmpty), new Cell() } },
    ];
}

public class Piece : IGameObject
{
    public Piece(Texture2D texture, Grid grid, Cell[,] shape = null!)
    {
        _texture = texture;
        _grid = grid;
        x = grid.Width / 2;
        y = 0;
        matrix = shape ?? PieceShapes.All[0];

        OnSpawned?.Invoke();
    }
    private readonly Texture2D _texture;
    private readonly Grid _grid;

    private Cell[,] matrix;

    private int x;
    private int y;

    private double fallTimer = 0;
    private readonly double fallInterval = 0.5; // seconds between drops

    public event Action OnLocked;
    public event Action OnSpawned;
    public event Action OnMoved;

    private bool TryMove(int dx, int dy)
    {
        int newX = x + dx;
        int newY = y + dy;

        if (!_grid.IsValidPosition(newX, newY, matrix)) return false;

        x = newX;
        y = newY;
        OnMoved?.Invoke();
        return true;
    }

    private bool TryRotate()
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        var rotated = new Cell[cols, rows];

        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
            {
                Cell src = matrix[i, j];
                if (src.State != CellState.Empty)
                {
                    RotationState newRot = (RotationState)(((int)src.Rotation + 90) % 360);
                    rotated[j, rows - 1 - i] = new Cell(src.State, newRot);
                }
                else
                {
                    rotated[j, rows - 1 - i] = new Cell();
                }
            }

        if (!_grid.IsValidPosition(x, y, rotated)) return false;

        matrix = rotated;
        return true;
    }

    private void Lock()
    {
        _grid.PlacePiece(x, y, matrix);

        OnLocked?.Invoke();
    }

    public void Update(GameTime gameTime)
    {
        fallTimer += gameTime.ElapsedGameTime.TotalSeconds;
        if (fallTimer >= fallInterval)
        {
            if (!TryMove(0, 1)) Lock();
            fallTimer = 0;
        }

        if (KeyboardInfo.WasKeyJustPressed(Keys.Left)) TryMove(-1, 0);
        if (KeyboardInfo.WasKeyJustPressed(Keys.Right)) TryMove(1, 0);
        if (KeyboardInfo.WasKeyJustPressed(Keys.Down))
        {
            if (!TryMove(0, 1)) Lock();
        }
        if (KeyboardInfo.WasKeyJustPressed(Keys.Space))
        {
            while (TryMove(0, 1)) { }
            Lock();
        }
        if (KeyboardInfo.WasKeyJustPressed(Keys.R) || KeyboardInfo.WasKeyJustPressed(Keys.Up))
        {
            TryRotate();
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        int pixelX = _grid.OffsetX + x * _grid.CellSize;
        int pixelY = _grid.OffsetY + y * _grid.CellSize;

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                if (matrix[i, j].State != CellState.Empty)
                {
                    Cell cell = matrix[i, j];
                    Rectangle sourceRect = cell.SourceRect;
                    int drawX = pixelX + i * _grid.CellSize;
                    int drawY = pixelY + j * _grid.CellSize;
                    int rotation = (int)cell.Rotation;
                    Vector2 origin = new(_grid.CellSize / 2, _grid.CellSize / 2);
                    spriteBatch.Draw(_texture, new Rectangle(drawX + _grid.CellSize / 2, drawY + _grid.CellSize / 2, _grid.CellSize, _grid.CellSize), sourceRect, Color.White, rotation * (float)(Math.PI / 180), origin, SpriteEffects.None, 0f);
                }
            }
        }
    }
}