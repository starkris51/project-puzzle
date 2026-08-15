using System;
using System.Collections.Generic;
using Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

public class GameScene(ContentManager content) : IScene
{
    private readonly ContentManager _content = content;

    private Grid grid = null!;
    private Texture2D tileset = null!;

    private readonly List<Piece> pieces = [];

    private void SpawnPiece()
    {
        pieces.Add(new Piece(tileset, grid));
    }

    public void Load()
    {
        tileset = _content.Load<Texture2D>("TilesetV5");

        grid = new Grid(tileset, 960, 540);

        pieces.Clear();
        SpawnPiece();

        grid.OnGameOver += Restart;
        grid.RequestNewPiece += SpawnPiece;
    }

    public void Unload()
    {
        // Unload game assets here
        _content.Unload();
    }

    private void Restart()
    {
        grid.Reset();
        pieces.Clear();
        SpawnPiece();
    }

    public void Update(GameTime gameTime)
    {
        KeyboardInfo.Update();
        grid?.Update(gameTime);

        if (grid?.IsGameOver ?? true) return;

        // Walk backwards over the pieces that existed at the start of the frame: locking a
        // piece spawns its replacement at the end of the list, which must not be updated
        // again this frame.
        for (int i = pieces.Count - 1; i >= 0; i--)
        {
            Piece p = pieces[i];
            p.Update(gameTime);
            if (p.IsLocked) pieces.RemoveAt(i);
        }
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        grid?.Draw(spriteBatch);

        foreach (Piece p in pieces)
            p.Draw(spriteBatch);
    }
}