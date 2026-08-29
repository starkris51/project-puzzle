using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay;

// Draws a Cell[,] matrix at any size, including sizes that aren't a whole multiple of the
// tileset's native tile size. Drawing tiles directly at a fractional scale with point
// sampling stretches them unevenly and can bleed in neighboring tiles from the packed
// tileset. Instead, this renders the matrix pixel-perfect (native tile size, point
// sampling) into an isolated buffer, then does a single smooth (linear-filtered) scale of
// that buffer into the destination rectangle — e.g. for a shrunk overlay on top of a board.
public class CellMatrixRenderer(Texture2D tileset)
{
    private readonly Texture2D _tileset = tileset;
    private RenderTarget2D _buffer;

    // Must be called while spriteBatch is NOT in an active Begin/End block (swapping
    // render targets mid-batch is invalid). Leaves an active PointClamp batch running
    // afterward, matching the convention the rest of the scene's Draw calls expect.
    public void Draw(SpriteBatch spriteBatch, Cell[,] matrix, Rectangle destination)
    {
        GraphicsDevice graphicsDevice = spriteBatch.GraphicsDevice;

        int nativeSize = CellTexture.CellSize;
        int width = matrix.GetLength(0) * nativeSize;
        int height = matrix.GetLength(1) * nativeSize;

        if (_buffer == null || _buffer.Width != width || _buffer.Height != height)
        {
            _buffer?.Dispose();
            _buffer = new RenderTarget2D(graphicsDevice, width, height);
        }

        RenderTargetBinding[] previousTargets = graphicsDevice.GetRenderTargets();

        graphicsDevice.SetRenderTarget(_buffer);
        graphicsDevice.Clear(Color.Transparent);

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        for (int x = 0; x < matrix.GetLength(0); x++)
        {
            for (int y = 0; y < matrix.GetLength(1); y++)
            {
                Cell cell = matrix[x, y];
                if (cell == null || cell.State == CellState.Empty) continue;

                spriteBatch.Draw(_tileset, new Rectangle(x * nativeSize, y * nativeSize, nativeSize, nativeSize), cell.SourceRect, Color.White);
            }
        }
        spriteBatch.End();

        graphicsDevice.SetRenderTargets(previousTargets);

        spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        spriteBatch.Draw(_buffer, destination, Color.White);
        spriteBatch.End();

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
    }
}
