using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content; // For ContentManager
using System.Collections.Generic;

namespace MyFirstGame
{
    // Manages game state
    public enum GameState
    {
        MainMenu,
        Playing,
        GameOver
    }

    // This is a normal class. It does NOT inherit from Game.
    public class GameManager
    {
        private SpriteFont uiFont;
        public GameState CurrentState { get; private set; }

        // Entities
        private Player player;
        private List<Level> levels;
        private List<Projectile> projectiles;

        // Level tracking
        private int currentLevelIndex;

        // Stored reference to GraphicsDevice
        private GraphicsDevice graphicsDevice;

        // Textures (Assets)
        // private Texture2D playerTexture;
        // private Texture2D projectileTexture;
        // private Texture2D scoutTexture;
        // private Texture2D fighterTexture;
        // private Texture2D bossTexture;
        private Texture2D placeholderTexture;

        public GameManager()
        {
            CurrentState = GameState.Playing; // Start playing immediately
            levels = new List<Level>();
            projectiles = new List<Projectile>();
            currentLevelIndex = 0;
        }

        // This is a PUBLIC method. No "override".
        public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
        {
            // Store the graphics device so we can use it in Update()
            this.graphicsDevice = graphicsDevice;
            
            // Load textures
            // playerTexture = content.Load<Texture2D>("player_ship");
            // projectileTexture = content.Load<Texture2D>("bullet");
            // scoutTexture = content.Load<Texture2D>("alien_scout");
            // fighterTexture = content.Load<Texture2D>("alien_fighter");
            // bossTexture = content.Load<Texture2D>("alien_dragon_boss");
            placeholderTexture = new Texture2D(graphicsDevice, 1, 1);
            placeholderTexture.SetData(new Color[] { Color.White });

            // Create player
            Vector2 startPos = new Vector2(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height - 100);

            // Use the 4-parameter constructor we fixed
            // player = new Player("Captain Affwaz", playerTexture, projectileTexture, startPos);
            player = new Player("Captain Affwaz", placeholderTexture, placeholderTexture, startPos); 

            // Create levels
            // levels.Add(new Level(1, scoutTexture, fighterTexture, bossTexture)); 
            // levels.Add(new Level(2, scoutTexture, fighterTexture, bossTexture)); 
            // levels.Add(new Level(3, scoutTexture, fighterTexture, bossTexture)); 
            // levels.Add(new Level(4, scoutTexture, fighterTexture, bossTexture)); 
            levels.Add(new Level(1, placeholderTexture, placeholderTexture, placeholderTexture)); 
            levels.Add(new Level(2, placeholderTexture, placeholderTexture, placeholderTexture)); 
            levels.Add(new Level(3, placeholderTexture, placeholderTexture, placeholderTexture)); 
            levels.Add(new Level(4, placeholderTexture, placeholderTexture, placeholderTexture));

            // Load the first level's enemies
            levels[currentLevelIndex].Load();
            uiFont = content.Load<SpriteFont>("UIFont");
        }

        // This is a PUBLIC method. No "override".
        public void Update(GameTime gameTime)
        {
            if (CurrentState == GameState.Playing)
            {
                // 1. Update Player
                player.Update(gameTime);

                // 2. Update Current Level (which updates all enemies)
                Level currentLevel = levels[currentLevelIndex];
                currentLevel.Update(gameTime, player);

                // 3. Update Projectiles (pass graphicsDevice as fixed)
                foreach (var p in projectiles)
                {
                    p.Update(gameTime, this.graphicsDevice);
                }

                // 4. Collision detection logic
                // Level currentLevel = levels[currentLevelIndex];

                // --- Projectile vs Enemy ---
                foreach (var projectile in projectiles)
                {
                    if (!projectile.IsActive)
                        continue;

                    foreach (var enemy in currentLevel.Enemies)
                    {
                        if (!enemy.IsActive)
                            continue;

                        if (projectile.BoundingBox.Intersects(enemy.BoundingBox))
                        {
                            projectile.OnHit(); // deactivate the projectile
                            enemy.TakeDamage(projectile.Damage);
                        }
                    }
                }

                // --- Enemy vs Player ---
                foreach (var enemy in currentLevel.Enemies)
                {
                    if (enemy.IsActive && enemy.BoundingBox.Intersects(player.BoundingBox))
                    {
                        player.TakeDamage(10);       // player loses HP
                        // optional instant enemy death
                        // enemy.TakeDamage(100);
                    }
                }

                // cleanup inactive stuff
                projectiles.RemoveAll(p => !p.IsActive);
                currentLevel.Enemies.RemoveAll(e => !e.IsActive);

                
                // 5. Check for game over
                if (player.HP <= 0)
                {
                    CurrentState = GameState.GameOver;
                }
            }
            else
            {
                // (Logic for MainMenu or GameOver screen)
            }
        }

        // This is a PUBLIC method. No "override".
        public void Draw(SpriteBatch spriteBatch)
        {
            if (CurrentState == GameState.Playing)
            {
                // 1. Draw Player
                player.Draw(spriteBatch);
                
                // 2. Draw Level (Enemies and PowerUps)
                levels[currentLevelIndex].Draw(spriteBatch);
                
                // 3. Draw Projectiles
                foreach (var p in projectiles) 
                { 
                    if (p.IsActive)
                    {
                        p.Draw(spriteBatch); 
                    }
                }
                string hpText = $"HP: {player.HP}";
                string weaponText = $"Weapon Lvl: {player.WeaponLevel}";

                spriteBatch.DrawString(
                uiFont,       // The font
                hpText,       // The text
                new Vector2(10, 10), // The position (X, Y)
                Color.White   // The color
                );

                spriteBatch.DrawString(
                uiFont,
                weaponText,
                new Vector2(10, 30), // Y is 30 (10 + 20)
                Color.White
                );
            }
            else
            {
                // (Logic to draw MainMenu or GameOver text)
            }
        }
    }
}