using System;
using Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

public class GameScene(ContentManager content) : IScene
{
    private readonly ContentManager _content = content;

    private Grid grid = null!;
    private Texture2D tileset = null!;

    private readonly Random random = new();

    private Piece piece = null!;

    private void SpawnPiece()
    {
        piece = new Piece(tileset, grid, PieceShapes.All[random.Next(PieceShapes.All.Length)]);
        piece.OnLocked += SpawnPiece;
    }

    public void Load()
    {
        tileset = _content.Load<Texture2D>("TilesetV4");

        grid = new Grid(tileset, 800, 600);
        SpawnPiece();
    }

    public void Unload()
    {
        // Unload game assets here
        _content.Unload();
    }

    public void Update(GameTime gameTime)
    {
        KeyboardInfo.Update();
        grid.Update(gameTime);
        piece.Update(gameTime);
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        grid?.Draw(spriteBatch);
        piece?.Draw(spriteBatch);
    }
}