
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public static class PieceShapes
{
    public static readonly int[][,] BaseShapes =
    [
        new int[,] { { 0, 1, 0 },
                      { 0, 1, 0 },
                      {0,0,0} },
        new int[,] { {0,0,0},
                        {1, 1, 1}, {0, 0, 1}},
        new int[,] { {0,0,0},
                      {1, 1, 1},
                      {1, 0, 0}},
        new int[,] {
                        {1, 1, 1},
                        {1, 0, 1},
                        {0,0,0}
                    },
        new int[,] {
            {1, 1, 0},
            {1, 0,0},
            {0,0,0}
        },
        new int[,] {
            {0, 1, 1},
            {0, 0,1},
            {0,0,0}
        },
    ];

    public static Cell[,] GetNewPiece()
    {
        Cell[,] pieceMatrix = new Cell[3, 3];

        Random random = new();

        var randomBaseShape = BaseShapes[random.Next(BaseShapes.Length)];

        CellState[] cellStates = [CellState.Symbol1, CellState.Symbol2, CellState.Symbol3];

        for (int i = 0; i < randomBaseShape.GetLength(0); i++)
        {
            for (int j = 0; j < randomBaseShape.GetLength(1); j++)
            {
                if (randomBaseShape[i, j] == 1)
                {
                    var randomCellStateIndex = random.Next(cellStates.Length);
                    var newCellState = cellStates[randomCellStateIndex];
                    pieceMatrix[i, j] = new Cell(newCellState);
                }
                else
                {
                    pieceMatrix[i, j] = new Cell();
                }
            }
        }
        return pieceMatrix;
    }
}

public class Piece
{
    public Piece(Texture2D texture, Grid grid)
    {
        _texture = texture;
        _grid = grid;
        x = (grid.Width / 2) - 1;
        y = 0;
        matrix = PieceShapes.GetNewPiece();

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
    public event Action OnRotated;

    private bool TryMove(int dx, int dy)
    {
        int newX = x + dx;
        int newY = y + dy;

        if (dx == 1 || dx == -1) SoundManager.Play(Sounds.MovePiece);

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
                    rotated[j, rows - 1 - i] = new Cell(src.State);
                }
                else
                {
                    rotated[j, rows - 1 - i] = new Cell();
                }
            }

        if (!_grid.IsValidPosition(x, y, rotated)) return false;

        matrix = rotated;
        OnRotated?.Invoke();
        SoundManager.Play(Sounds.Rotate);
        return true;
    }

    private void Lock()
    {
        _grid.PlacePiece(x, y, matrix);

        OnLocked?.Invoke();
        // SoundManager.Play(Sounds.PlacePiece); --- IGNORE ---
    }

    public void Update(GameTime gameTime)
    {
        if (_grid.IsAnimating) return;

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
                    Vector2 origin = new(_grid.CellSize / 2, _grid.CellSize / 2);
                    spriteBatch.Draw(_texture, new Rectangle(drawX + _grid.CellSize / 2, drawY + _grid.CellSize / 2, _grid.CellSize, _grid.CellSize), sourceRect, Color.White, 0f, origin, SpriteEffects.None, 0f);
                }
            }
        }
    }
}