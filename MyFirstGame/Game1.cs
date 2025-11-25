using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MyFirstGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics; 
        private SpriteBatch _spriteBatch;

        // The logic manager for the game
        private GameManager gameManager;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
    
            // Screen Resolution Settings
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 1000;
            _graphics.ApplyChanges();

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            gameManager = new GameManager();
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Pass Content and GraphicsDevice to the GameManager to load assets
            gameManager.LoadContent(this.Content, this.GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            // Exit functionality
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Delegate update logic to GameManager
            gameManager.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();
            gameManager.Draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}