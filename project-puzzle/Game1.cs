using Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace project_puzzle;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private ScalingWindow _scalingWindow;
    private SceneManager _sceneManager;

    private GameScene _gameScene;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _scalingWindow = new ScalingWindow(_graphics, Window, 800, 600);
    }

    protected override void Initialize()
    {
        base.Initialize();
        _scalingWindow.Initialize();

        _sceneManager = new SceneManager();

        _gameScene = new GameScene(Content);
        _sceneManager.Push(_gameScene);
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        SoundManager.Load(Content);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _sceneManager.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _scalingWindow.BeginDraw();

        _graphics.GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        _sceneManager.Draw(gameTime, _spriteBatch);

        _spriteBatch.End();

        _scalingWindow.EndDraw(_spriteBatch);

        base.Draw(gameTime);
    }
}
