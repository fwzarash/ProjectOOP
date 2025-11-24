using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MyFirstGame
{
    public class Game1 : Game
    {
        // This is required for MonoGame to access the graphics card
        private GraphicsDeviceManager _graphics; 
        
        private SpriteBatch _spriteBatch;

        // This is the *instance* of your logic class
        private GameManager gameManager;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
    
            // --- CHANGE THESE NUMBERS TO RESIZE THE WINDOW ---
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 1000;
            // _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();
            // -------------------------------------------------

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            gameManager = new GameManager();
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        // This signature is correct: override, no parameters
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Pass the MonoGame Content and GraphicsDevice to your manager
            gameManager.LoadContent(this.Content, this.GraphicsDevice);
        }

        // This signature is correct: override, GameTime parameter
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Tell your game manager to update its logic
            gameManager.Update(gameTime);

            base.Update(gameTime);
        }

        // This signature is correct: override, GameTime parameter
        protected override void Draw(GameTime gameTime)
        {
            // Change background to Black for a space shooter
            GraphicsDevice.Clear(Color.Black);

            // Begin the sprite batch
            _spriteBatch.Begin();
            
            // Tell your game manager to draw everything
            gameManager.Draw(_spriteBatch);
            
            // End the sprite batch
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}