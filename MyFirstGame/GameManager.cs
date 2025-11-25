using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio; 
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;
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
        // --- Core Systems ---
        public GameState CurrentState { get; private set; }
        private GraphicsDevice graphicsDevice;
        private Random random = new Random();
        
        // --- Entities & Lists ---
        private Player player;
        private List<Level> levels;
        private List<Projectile> projectiles;
        private List<Projectile> enemyProjectiles;
        public List<Projectile> EnemyProjectile { get { return enemyProjectiles; } }
        public List<PowerUp> powerUps = new List<PowerUp>();
        
        // --- Assets ---
        private Texture2D playerTexture;
        private Texture2D projectileTexture;
        public Texture2D EnemyProjectileTexture { get; private set; }
        private Texture2D scoutTexture;
        private Texture2D fighterTexture;
        private Texture2D bossTexture;
        private Texture2D backgroundTexture;
        private Texture2D powerUpTexture;
        private Texture2D placeholderTexture; // For Health bars
        private SpriteFont uiFont;

        // --- Audio ---
        private List<Song> levelSongs;
        private SoundEffect shootSound;
        private SoundEffect hurtSound;
        private SoundEffect impactSound;
        private SoundEffect pressStart;
        private SoundEffect reloadSound;
        
        // --- Audio Transition Logic ---
        private bool isTransitioning = false;
        private int nextLevelIndex = -1;
        private float fadeSpeed = 1.0f; 
        private float maxVolume = 0.3f; 
        private bool isFadingOut = false;

        // --- Level & Environment ---
        private float _backgroundScrollY = 0f;
        private int currentLevelIndex;
        public int ScreenWidth { get { return graphicsDevice.Viewport.Width; } }
        public int ScreenHeight { get { return graphicsDevice.Viewport.Height; } }
        
        // Safety timer to prevent instant collision on spawn
        private float collisionDelay = 1f; 
        private float elapsedSinceLevelStart = 0f;

        // Input
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
            
            // 1. Load Audio
            pressStart = content.Load<SoundEffect>("audio/sfx/snd_select");
            shootSound = content.Load<SoundEffect>("audio/sfx/snd_heartshot");
            hurtSound = content.Load<SoundEffect>("audio/sfx/snd_hurt1");
            impactSound = content.Load<SoundEffect>("audio/sfx/snd_impact");
            reloadSound = content.Load<SoundEffect>("sci-fi-charge-up-37395");

            levelSongs = new List<Song>();
            try 
            {
                levelSongs.Add(content.Load<Song>("audio/st/mus_core[1]"));
                levelSongs.Add(content.Load<Song>("audio/st/mus_mettaton_ex[1]"));
                levelSongs.Add(content.Load<Song>("audio/st/Gathers_Under_Night..._[J8pBhDfERGk][1]"));
                levelSongs.Add(content.Load<Song>("audio/st/Pandemonium_[qOlU6Lsmvvs][1]"));
            }
            catch { System.Diagnostics.Debug.WriteLine("Music files missing!"); }

            // 2. Load Textures (with fallbacks for robustness)
            try { playerTexture = content.Load<Texture2D>("player_ship"); }
            catch { playerTexture = CreateColoredTexture(32, Color.White); }

            try { projectileTexture = content.Load<Texture2D>("bullet"); }
            catch { projectileTexture = CreateColoredTexture(5, Color.Yellow); }
            
            // Ensure bullet is visible if texture failed
            if (projectileTexture == null || projectileTexture.Width == 0)
                projectileTexture = CreateColoredTexture(5, Color.Yellow);

            try { scoutTexture = content.Load<Texture2D>("alien_scout"); } 
            catch { scoutTexture = CreateColoredTexture(32, Color.Green); }

            try { fighterTexture = content.Load<Texture2D>("alien_fighter"); } 
            catch { fighterTexture = CreateColoredTexture(32, Color.Blue); }

            try { bossTexture = content.Load<Texture2D>("alien_boss_dragon"); } 
            catch { bossTexture = CreateColoredTexture(128, Color.Purple); }

            try { backgroundTexture = content.Load<Texture2D>("space_background"); }
            catch { backgroundTexture = CreateColoredTexture(800, Color.Black); }

            powerUpTexture = content.Load<Texture2D>("powerup_heart");
            EnemyProjectileTexture = CreateColoredTexture(6, Color.Red);

            // 3. Load UI
            try { uiFont = content.Load<SpriteFont>("UIFont"); }
            catch { throw new System.Exception("ERROR: Ensure UIFont.spritefont exists in MGCB Content!"); }

            placeholderTexture = new Texture2D(graphicsDevice, 1, 1);
            placeholderTexture.SetData(new Color[] { Color.White });

            ResetGame();
        }

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

        // --- Music Management ---
        private void PlayLevelMusic(int levelIndex)
        {
            if (levelSongs == null || levelIndex >= levelSongs.Count) return;

            if (MediaPlayer.State != MediaState.Playing)
            {
                // First time play (Fade In)
                MediaPlayer.Play(levelSongs[levelIndex]);
                MediaPlayer.Volume = 0f; 
                MediaPlayer.IsRepeating = true;
                isTransitioning = true;
                isFadingOut = false; 
                nextLevelIndex = -1;
            }
            else
            {
                // Transitioning tracks (Fade Out first)
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

        public void AddProjectile(Projectile p) => projectiles.Add(p);
        public void AddEnemyProjectile(Projectile p) => enemyProjectiles.Add(p);

        public void SpawnPowerUp(Vector2 position)
        {
            // Returns 0, 1, or 2
            int typeIndex = random.Next(3); 
            PowerUpType type;

            if (typeIndex == 0)
                type = PowerUpType.Health;
            else if (typeIndex == 1)
                type = PowerUpType.WeaponUpgrade;
            else
                type = PowerUpType.Shield;

            // Value 50 is generic, specific logic is handled in Apply()
            this.powerUps.Add(new PowerUp(powerUpTexture, position, type, 50));
        }

        public void Update(GameTime gameTime)
        {
            // --- Music Transitions ---
            if (isTransitioning)
            {
                float timePassed = (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (isFadingOut)
                {
                    MediaPlayer.Volume -= fadeSpeed * timePassed;
                    if (MediaPlayer.Volume <= 0f)
                    {
                        MediaPlayer.Stop();
                        if (nextLevelIndex != -1 && nextLevelIndex < levelSongs.Count)
                        {
                            MediaPlayer.Play(levelSongs[nextLevelIndex]);
                            MediaPlayer.IsRepeating = true;
                        }
                        isFadingOut = false; // Start Fading In
                    }
                }
                else 
                {
                    MediaPlayer.Volume += fadeSpeed * timePassed;
                    if (MediaPlayer.Volume >= maxVolume)
                    {
                        MediaPlayer.Volume = maxVolume;
                        isTransitioning = false;
                    }
                }
            }

            // --- Background Parallax ---
            _backgroundScrollY += 50f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_backgroundScrollY >= graphicsDevice.Viewport.Height) _backgroundScrollY = 0;

            KeyboardState kState = Keyboard.GetState();

            // --- State Machine ---
            switch (CurrentState)
            {
                case GameState.MainMenu:
                    if (IsKeyPressed(Keys.Enter)) 
                    {
                        if (pressStart != null) pressStart.Play();
                        CurrentState = GameState.Playing;
                        PlayLevelMusic(currentLevelIndex);
                    }
                    break;

                case GameState.GameOver:
                case GameState.Victory:
                    if (IsKeyPressed(Keys.Enter))
                    {
                        ResetGame();
                        CurrentState = GameState.MainMenu;
                    }
                    break;

                case GameState.Playing:
                    UpdateGameplay(gameTime, kState);
                    break;
            }

            previousKeyboardState = kState;
        }

        private void UpdateGameplay(GameTime gameTime, KeyboardState kState)
        {
            elapsedSinceLevelStart += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // 1. Update Entities
            player.Update(gameTime, graphicsDevice.Viewport.Bounds);
            Level currentLevel = levels[currentLevelIndex];
            currentLevel.Update(gameTime, player);

            // 2. Check Level Progression
            if (currentLevel.IsComplete) 
            {
                currentLevelIndex++;
                elapsedSinceLevelStart = 0f; 
                
                if (currentLevelIndex >= levels.Count)
                {
                    MediaPlayer.Stop();
                    CurrentState = GameState.Victory;
                }
                else
                {
                    levels[currentLevelIndex].Load(); 
                    PlayLevelMusic(currentLevelIndex);
                }
                return;
            }

            // 3. Player Actions
            if (player.CanShoot() && kState.IsKeyDown(Keys.Space))
            {
                Projectile bullet = player.Shoot();
                projectiles.Add(bullet);
                player.ResetCooldown();
                if (shootSound != null) shootSound.Play(0.5f, 0f, 0f);
            }

            // 4. Update Projectiles
            foreach (var p in projectiles) p.Update(gameTime, graphicsDevice);
            foreach (var ep in enemyProjectiles) if (ep.IsActive) ep.Update(gameTime, graphicsDevice);

            // 5. Collision: Player Bullets vs Enemies
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

            // 6. Collision: Enemy Bullets vs Player
            foreach (var ep in enemyProjectiles)
            {
                if (!ep.IsActive) continue;
                if (ep.BoundingBox.Intersects(player.BoundingBox))
                {
                    player.TakeDamage(ep.Damage);
                    if (hurtSound != null) hurtSound.Play();
                    ep.OnHit();
                }
            }

            // 7. Collision: Enemy Body vs Player (after initial delay)
            if (elapsedSinceLevelStart > collisionDelay)
            {
                foreach (var enemy in currentLevel.Enemies)
                {
                    if (enemy.IsActive && enemy.BoundingBox.Intersects(player.BoundingBox))
                    {
                        player.TakeDamage(10); 
                        enemy.TakeDamage(100); 
                    }
                }
            }

            // 8. Cleanup
            projectiles.RemoveAll(p => !p.IsActive);
            enemyProjectiles.RemoveAll(ep => !ep.IsActive);
            
            if (player.HP <= 0)
            {
                CurrentState = GameState.GameOver;
                isTransitioning = true;
                isFadingOut = true;
                nextLevelIndex = -1; // Fade to silence
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int screenWidth = graphicsDevice.Viewport.Width;
            int screenHeight = graphicsDevice.Viewport.Height;

            // --- Draw Infinite Scrolling Background ---
            // Draw first instance
            spriteBatch.Draw(backgroundTexture, new Rectangle(0, (int)_backgroundScrollY, screenWidth, screenHeight), Color.White);
            // Draw second instance immediately above it
            spriteBatch.Draw(backgroundTexture, new Rectangle(0, (int)_backgroundScrollY - screenHeight, screenWidth, screenHeight), Color.White);

            // --- Draw UI based on State ---
            if (CurrentState == GameState.MainMenu)
            {
                DrawCenteredText(spriteBatch, "SPACE SHOOTER\nPress ENTER to Start", Color.White);
            }
            else if (CurrentState == GameState.GameOver)
            {
                DrawCenteredText(spriteBatch, "GAME OVER\nPress ENTER to Restart", Color.Red);
            }
            else if (CurrentState == GameState.Victory)
            {
                DrawCenteredText(spriteBatch, "MISSION ACCOMPLISHED!\nYOU WIN!\nPress ENTER to Return", Color.Gold);
            }
            else if (CurrentState == GameState.Playing)
            {
                player.Draw(spriteBatch);
                levels[currentLevelIndex].Draw(spriteBatch);
                
                foreach (var p in projectiles) if (p.IsActive) p.Draw(spriteBatch); 
                foreach (var ep in enemyProjectiles) if (ep.IsActive) ep.Draw(spriteBatch);

                // Draw Boss Health Bar if applicable
                if (currentLevelIndex < levels.Count) 
                {
                    foreach (var enemy in levels[currentLevelIndex].Enemies)
                    {
                        if (enemy is BossAlienDragon boss)
                        {
                            spriteBatch.Draw(placeholderTexture, new Rectangle(200, 10, 400, 20), Color.Gray);
                            float healthPct = (float)boss.HP / 1000f; 
                            if (healthPct < 0) healthPct = 0;
                            spriteBatch.Draw(placeholderTexture, new Rectangle(200, 10, (int)(400 * healthPct), 20), Color.White);
                        }
                    }
                }

                // HUD
                spriteBatch.DrawString(uiFont, $"HP: {player.HP}", new Vector2(10, 10), Color.White);
                spriteBatch.DrawString(uiFont, $"Level: {currentLevelIndex + 1}", new Vector2(10, 30), Color.Yellow);
                spriteBatch.DrawString(uiFont, $"Ammo: {player.CurrentAmmo}/{player.MaxAmmo}", new Vector2(10, 50), Color.Cyan); 

                if (player.CurrentAmmo <= 0)
                {
                    string reloadMsg = "NO AMMO! Press 'R'";
                    Vector2 size = uiFont.MeasureString(reloadMsg);
                    spriteBatch.DrawString(uiFont, reloadMsg, new Vector2(screenWidth/2 - size.X/2, 400), Color.Red);
                }
            }
        }

        private void DrawCenteredText(SpriteBatch sb, string text, Color color)
        {
            Vector2 size = uiFont.MeasureString(text);
            Vector2 center = new Vector2(graphicsDevice.Viewport.Width / 2 - size.X / 2, graphicsDevice.Viewport.Height / 2 - size.Y / 2);
            sb.DrawString(uiFont, text, center, color);
        }
    }
}