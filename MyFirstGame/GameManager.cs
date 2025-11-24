using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio; 
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.CompilerServices;
using System;

namespace MyFirstGame
{
    public enum GameState
    {
        MainMenu,
        Playing,
        GameOver,
        Victory
    }

    public class GameManager
    {
        private SpriteFont uiFont;
        public GameState CurrentState { get; private set; }

        // Entities
        private float _backgroundScrollY = 0f;
        private Player player;
        private List<Level> levels;
        private List<Projectile> projectiles;
        private List<Projectile> enemyProjectiles;
        public List<Projectile> EnemyProjectile { get { return enemyProjectiles; } }
        public List<PowerUp> powerUps = new List<PowerUp>();
        private List<Song> levelSongs;
        private Texture2D powerUpTexture;

        // Add these public properties so other classes can read the screen size
        public int ScreenWidth { get { return graphicsDevice.Viewport.Width; } }
        public int ScreenHeight { get { return graphicsDevice.Viewport.Height; } }

        // Level tracking
        private int currentLevelIndex;

        // Graphics & Assets
        private GraphicsDevice graphicsDevice;
        private Texture2D playerTexture;
        private Texture2D projectileTexture;
        private Texture2D scoutTexture;
        private Texture2D fighterTexture;
        private Texture2D bossTexture;
        private Texture2D backgroundTexture;
        private Texture2D placeholderTexture;
        private Texture2D enemyProjectileTexture;

        // mus
        private bool isTransitioning = false;
        private int nextLevelIndex = -1;
        private float fadeSpeed = 1.0f; // Higher = faster fade
        private float maxVolume = 0.3f; // normal volume
        private bool isFadingOut = false;
        private SoundEffect shootSound;
        private SoundEffect hurtSound;
        private SoundEffect impactSound;
        private SoundEffect pressStart;
        private SoundEffect reloadSound;
        public Texture2D EnemyProjectileTexture { get { return enemyProjectileTexture; } }

        // --- Added for safe spawn ---
        private float collisionDelay = 1f; 
        private float elapsedSinceLevelStart = 0f;

        private KeyboardState previousKeyboardState;

        public GameManager()
        {
            CurrentState = GameState.MainMenu;
            levels = new List<Level>();
            projectiles = new List<Projectile>();
            enemyProjectiles = new List<Projectile>();
            currentLevelIndex = 0;
            previousKeyboardState = Keyboard.GetState();
        }

        public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            pressStart = content.Load<SoundEffect>("audio/sfx/snd_select");
            
            // 1. Load Player (Fallback to White if missing)
            try { playerTexture = content.Load<Texture2D>("player_ship"); }
            catch { playerTexture = CreateColoredTexture(32, Color.White); }

            levelSongs = new List<Song>();
            // Load Sountracks for levels
            try 
            {
                // Load a different song for each level
                levelSongs.Add(content.Load<Song>("audio/st/mus_core[1]"));
                levelSongs.Add(content.Load<Song>("audio/st/mus_mettaton_ex[1]"));
                levelSongs.Add(content.Load<Song>("audio/st/Gathers_Under_Night..._[J8pBhDfERGk][1]"));
                levelSongs.Add(content.Load<Song>("audio/st/Pandemonium_[qOlU6Lsmvvs][1]"));
            }
            catch 
            {
                System.Diagnostics.Debug.WriteLine("Music files missing!");
            }
            
            // 2. Load Bullet (Fallback to Orange if missing)
            reloadSound = content.Load<SoundEffect>("sci-fi-charge-up-37395");

            try 
            {
                projectileTexture = content.Load<Texture2D>("bullet");
                shootSound = content.Load<SoundEffect>("audio/sfx/snd_heartshot");
                hurtSound = content.Load<SoundEffect>("audio/sfx/snd_hurt1");
                impactSound = content.Load<SoundEffect>("audio/sfx/snd_impact");
            }
            catch 
            {
                projectileTexture = null; 
            }

            // ================================================================
            // (FIX) Jadikan bullet lebih besar (20px) dan KUNING supaya jelas
            // ================================================================
            if (projectileTexture == null || projectileTexture.Width == 0)
            {
                projectileTexture = CreateColoredTexture(5, Color.Yellow);
            }
            // ================================================================

            // 3. Load Enemies (Fallback to colors if missing)
            try { scoutTexture = content.Load<Texture2D>("alien_scout"); } 
            catch { scoutTexture = CreateColoredTexture(32, Color.Green); }

            try { fighterTexture = content.Load<Texture2D>("alien_fighter"); } 
            catch { fighterTexture = CreateColoredTexture(32, Color.Blue); }

            try { bossTexture = content.Load<Texture2D>("alien_boss_dragon"); } 
            catch { bossTexture = CreateColoredTexture(128, Color.Purple); }
            
            // 4. Load Misc
            enemyProjectileTexture = CreateColoredTexture(6, Color.Red);
            
            try { backgroundTexture = content.Load<Texture2D>("space_background"); }
            catch { backgroundTexture = CreateColoredTexture(800, Color.Black); }

            powerUpTexture = content.Load<Texture2D>("powerup_heart");

            try { uiFont = content.Load<SpriteFont>("UIFont"); }
            catch { throw new System.Exception("ERROR: Ensure UIFont.spritefont exists in MGCB Content!"); }

            placeholderTexture = new Texture2D(graphicsDevice, 1, 1);
            placeholderTexture.SetData(new Color[] { Color.White });

            // Initialize the game objects
            ResetGame();
        }

        // Helper to create colored box textures
        private Texture2D CreateColoredTexture(int size, Color color)
        {
            if (graphicsDevice == null) return null;
            Texture2D tex = new Texture2D(graphicsDevice, size, size);
            Color[] data = new Color[size * size];
            for(int i=0; i<data.Length; i++) data[i] = color;
            tex.SetData(data);
            return tex;
        }

        private bool IsKeyPressed(Keys key)
        {
            KeyboardState current = Keyboard.GetState();
            return current.IsKeyDown(key) && !previousKeyboardState.IsKeyDown(key);
        }

        private void PlayLevelMusic(int levelIndex)
        {
            // Safety check
            if (levelSongs == null || levelIndex >= levelSongs.Count) return;

            // If music is already off (first start), just play immediately
            if (MediaPlayer.State != MediaState.Playing)
            {
                MediaPlayer.Play(levelSongs[levelIndex]);
                MediaPlayer.Volume = 0f; // Start silent
                MediaPlayer.IsRepeating = true;
                isTransitioning = true;
                isFadingOut = false; // Start by fading IN
                nextLevelIndex = -1; // No pending switch
            }
            else
            {
                // If music is playing, start the Fade Out process
                nextLevelIndex = levelIndex;
                isTransitioning = true;
                isFadingOut = true;
            }
        }

        private void ResetGame()
        {
            Vector2 startPos = new Vector2(
                (graphicsDevice.Viewport.Width / 2) - 64, 
                graphicsDevice.Viewport.Height - 100
            );
            
            player = new Player("Captain Affwaz", playerTexture, projectileTexture, startPos, reloadSound);

            levels.Clear();
            levels.Add(new Level(1, scoutTexture, fighterTexture, bossTexture, this)); 
            levels.Add(new Level(2, scoutTexture, fighterTexture, bossTexture, this)); 
            levels.Add(new Level(3, scoutTexture, fighterTexture, bossTexture, this)); 
            levels.Add(new Level(4, scoutTexture, fighterTexture, bossTexture, this));

            currentLevelIndex = 0;
            levels[currentLevelIndex].Load();
            
            elapsedSinceLevelStart = 0f;
            projectiles.Clear();
            enemyProjectiles.Clear();
        }

        public void AddProjectile(Projectile p)
        {
            projectiles.Add(p);

        }

        public void AddEnemyProjectile(Projectile p)
        {
            enemyProjectiles.Add(p);
        }

        public void Update(GameTime gameTime)
        {
            // transition music logic
            if (isTransitioning)
            {
                float timePassed = (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (isFadingOut)
                {
                    // Lower volume
                    MediaPlayer.Volume -= fadeSpeed * timePassed;

                    // Once silent, switch songs
                    if (MediaPlayer.Volume <= 0f)
                    {
                        MediaPlayer.Stop();
                        
                        // Switch the track
                        if (nextLevelIndex != -1 && nextLevelIndex < levelSongs.Count)
                        {
                            MediaPlayer.Play(levelSongs[nextLevelIndex]);
                            MediaPlayer.IsRepeating = true;
                        }
                        
                        // Start Fading In
                        isFadingOut = false; 
                    }
                }
                else // Fading In
                {
                    // Raise volume
                    MediaPlayer.Volume += fadeSpeed * timePassed;

                    // Target reached? Stop transitioning
                    if (MediaPlayer.Volume >= maxVolume)
                    {
                        MediaPlayer.Volume = maxVolume;
                        isTransitioning = false;
                    }
                }
            }
            // Background Scrolling
            _backgroundScrollY += 50f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_backgroundScrollY >= graphicsDevice.Viewport.Height) _backgroundScrollY = 0;

            KeyboardState kState = Keyboard.GetState();

            if (CurrentState == GameState.MainMenu)
            {
                if (IsKeyPressed(Keys.Enter)) {
                    if (pressStart != null) pressStart.Play();
        
                    CurrentState = GameState.Playing;
                    // music starts for Level 1
                    PlayLevelMusic(currentLevelIndex);
                }
            }
            else if (CurrentState == GameState.GameOver)
            {
                if (IsKeyPressed(Keys.Enter))
                {
                    ResetGame();
                    CurrentState = GameState.MainMenu;
                }
            }
            else if (CurrentState == GameState.Victory)
            {
                if (IsKeyPressed(Keys.Enter))
                {
                    ResetGame(); // Reset everything (Level 1, full HP)
                    CurrentState = GameState.MainMenu; // Go back to title
                }
            }
            else if (CurrentState == GameState.Playing)
            {
                elapsedSinceLevelStart += (float)gameTime.ElapsedGameTime.TotalSeconds;

                // 1. Update Player & Level
                player.Update(gameTime, graphicsDevice.Viewport.Bounds);
                Level currentLevel = levels[currentLevelIndex];
                currentLevel.Update(gameTime, player);

                // --- LEVEL PROGRESSION CHECK ---
                if (currentLevel.IsComplete) 
                {
                    currentLevelIndex++;
                    elapsedSinceLevelStart = 0f; // Reset collision safety timer
                    
                    if (currentLevelIndex >= levels.Count)
                    {
                        MediaPlayer.Stop();
                        CurrentState = GameState.Victory; // You Win (Back to menu)
                        return;
                    }
                    else
                    {
                        levels[currentLevelIndex].Load(); 
                        PlayLevelMusic(currentLevelIndex);
                    }
                }

                // 2. Player Shooting
                if (player.CanShoot() && kState.IsKeyDown(Keys.Space))
                {
                    Projectile bullet = player.Shoot();
                    projectiles.Add(bullet);
                    player.ResetCooldown();
                    if (shootSound != null)
                    {
                        shootSound.Play(0.5f, 0f, 0f);
                    }
                }

                // 3. Update Projectiles
                foreach (var p in projectiles) p.Update(gameTime, graphicsDevice);
                
                // 4. Update Enemy Projectiles
                foreach (var ep in enemyProjectiles)
                {
                     if (ep.IsActive) ep.Update(gameTime, graphicsDevice);
                }

                // 5. Collision: Player Bullet vs Enemies
                foreach (var projectile in projectiles)
                {
                    if (!projectile.IsActive) continue;

                    foreach (var enemy in currentLevel.Enemies)
                    {
                        if (!enemy.IsActive) continue;

                        if (projectile.BoundingBox.Intersects(enemy.BoundingBox))
                        {
                            projectile.OnHit(); 
                            if (impactSound != null) impactSound.Play();
                            enemy.TakeDamage(projectile.Damage);
                        }
                    }
                }

                // 6. Collision: Enemy Bullet vs Player
                foreach (var ep in enemyProjectiles)
                {
                    if (!ep.IsActive) continue;

                    if (ep.BoundingBox.Intersects(player.BoundingBox))
                    {
                        player.TakeDamage(ep.Damage);
                        if (hurtSound != null)
                        {
                            hurtSound.Play();
                        }
                        ep.OnHit();
                    }
                }

                // 7. Collision: Enemy Body vs Player (with Spawn Safety Delay)
                if (elapsedSinceLevelStart > collisionDelay)
                {
                    foreach (var enemy in currentLevel.Enemies)
                    {
                        if (enemy.IsActive && enemy.BoundingBox.Intersects(player.BoundingBox))
                        {
                            player.TakeDamage(10); // Contact damage
                            enemy.TakeDamage(100); // Enemy also takes damage/dies
                        }
                    }
                }

                // 8. Cleanup and Game Over Check
                projectiles.RemoveAll(p => !p.IsActive);
                enemyProjectiles.RemoveAll(ep => !ep.IsActive);
                
                if (player.HP <= 0)
                {
                    CurrentState = GameState.GameOver;
                    
                    //trigger a fade out
                    isTransitioning = true;
                    isFadingOut = true;
                    nextLevelIndex = -1; // -1 means "Don't play anything next, just silence"
                }
            }

            previousKeyboardState = kState;
        }

        // method for SpawnPowerUp
        private Random random = new Random();
        public void SpawnPowerUp(Vector2 position)
        {
            PowerUpType type;
            int typeRand = random.Next(2);
            if (typeRand == 0)
            {
                type = PowerUpType.Health;
            }
            else
            {
                type = PowerUpType.WeaponUpgrade;
            }
            
            int powerUpValue = 50;
            var newPowerUp = new PowerUp(
                    powerUpTexture,
                    position,
                    type,
                    powerUpValue
            );

            this.powerUps.Add(newPowerUp);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Get current screen dimensions
            int screenWidth = graphicsDevice.Viewport.Width;
            int screenHeight = graphicsDevice.Viewport.Height;

            // Image 1 (Moving down)
            // We create a Rectangle(x, y, width, height) to force it to fit the screen
            spriteBatch.Draw(
                backgroundTexture, 
                new Rectangle(0, (int)_backgroundScrollY, screenWidth, screenHeight), 
                Color.White
            );

            // Image 2 (The one following above)
            // We position it exactly one 'screenHeight' above the first one
            spriteBatch.Draw(
                backgroundTexture, 
                new Rectangle(0, (int)_backgroundScrollY - screenHeight, screenWidth, screenHeight), 
                Color.White
            );

            // // Scrolling Background
            // spriteBatch.Draw(backgroundTexture, new Vector2(0, _backgroundScrollY), Color.White);
            // spriteBatch.Draw(backgroundTexture, new Vector2(0, _backgroundScrollY - backgroundTexture.Height), Color.White);

            if (CurrentState == GameState.MainMenu)
            {
                string msg = "SPACE SHOOTER\nPress ENTER to Start";
                Vector2 size = uiFont.MeasureString(msg);
                Vector2 center = new Vector2(graphicsDevice.Viewport.Width / 2 - size.X / 2, graphicsDevice.Viewport.Height / 2);
                spriteBatch.DrawString(uiFont, msg, center, Color.White);
            }
            else if (CurrentState == GameState.GameOver)
            {
                string msg = "GAME OVER\nPress ENTER to Restart";
                Vector2 size = uiFont.MeasureString(msg);
                Vector2 center = new Vector2(graphicsDevice.Viewport.Width / 2 - size.X / 2, graphicsDevice.Viewport.Height / 2);
                spriteBatch.DrawString(uiFont, msg, center, Color.White);
            }
            else if (CurrentState == GameState.Victory)
            {
                string title = "MISSION ACCOMPLISHED!";
                string sub = "YOU WIN!\nPress ENTER to Return";
                
                // Measure text to center it
                Vector2 titleSize = uiFont.MeasureString(title);
                Vector2 subSize = uiFont.MeasureString(sub);
                
                Vector2 titlePos = new Vector2(
                    (graphicsDevice.Viewport.Width / 2) - (titleSize.X / 2), 
                    (graphicsDevice.Viewport.Height / 2) - 100
                );

                Vector2 subPos = new Vector2(
                    (graphicsDevice.Viewport.Width / 2) - (subSize.X / 2), 
                    graphicsDevice.Viewport.Height / 2
                );
                spriteBatch.DrawString(uiFont, title, titlePos, Color.Gold);
                spriteBatch.DrawString(uiFont, sub, subPos, Color.White);
            }
            else if (CurrentState == GameState.Playing)
            {
                player.Draw(spriteBatch);
                levels[currentLevelIndex].Draw(spriteBatch);
                
                foreach (var p in projectiles) if (p.IsActive) p.Draw(spriteBatch); 
                foreach (var ep in enemyProjectiles) if (ep.IsActive) ep.Draw(spriteBatch);

                // BOSS HEALTH BAR
                if (currentLevelIndex < levels.Count) 
                {
                    foreach (var enemy in levels[currentLevelIndex].Enemies)
                    {
                        if (enemy is BossAlienDragon boss)
                        {
                            // Gray Back
                            spriteBatch.Draw(placeholderTexture, new Rectangle(200, 10, 400, 20), Color.Gray);
                            
                            // Red Front
                            float healthPct = (float)boss.HP / 5000f; 
                            if (healthPct < 0) healthPct = 0;
                            spriteBatch.Draw(placeholderTexture, new Rectangle(200, 10, (int)(400 * healthPct), 20), Color.White);
                        }
                    }
                }


                // ===============================================
                // UI: HP, Level, and AMMO display
                // ===============================================
                string hpText = $"HP: {player.HP}";
                string lvlText = $"Level: {currentLevelIndex + 1}";
                string ammoText = $"Ammo: {player.CurrentAmmo}/{player.MaxAmmo}";
                
                spriteBatch.DrawString(uiFont, hpText, new Vector2(10, 10), Color.White);
                spriteBatch.DrawString(uiFont, lvlText, new Vector2(10, 30), Color.Yellow);
                spriteBatch.DrawString(uiFont, ammoText, new Vector2(10, 50), Color.Cyan); 

                // Amaran Ammo Habis
                if (player.CurrentAmmo <= 0)
                {
                    string reloadMsg = "NO AMMO! Press 'R'";
                    spriteBatch.DrawString(uiFont, reloadMsg, new Vector2(graphicsDevice.Viewport.Width/2 - 100, 400), Color.White);
                }
            }
        }
    }
}