using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace MyFirstGame
{
    public class Level
    {
        public int LevelNumber { get; private set; }
        public List<Enemy> Enemies { get; private set; }
        public List<PowerUp> PowerUps { get; private set; }

        private Texture2D alienScoutTexture;
        private Texture2D alienFighterTexture;
        private Texture2D bossTexture;
        private GameManager gameManager;

        // Level Completion Logic
        // Ensure we don't complete the level before enemies have actually spawned
        private bool hasEnemiesSpawned = false;
        public bool IsComplete 
        { 
            get { return hasEnemiesSpawned && Enemies.Count == 0; } 
        }

        private float spawnDelay = 1.0f; 
        private float elapsedTime = 0f;

        public Level(int number, Texture2D scoutTex, Texture2D fighterTex, Texture2D bossTex, GameManager gm)
        {
            this.LevelNumber = number;
            this.Enemies = new List<Enemy>();
            this.PowerUps = new List<PowerUp>();

            this.alienScoutTexture = scoutTex;
            this.alienFighterTexture = fighterTex;
            this.bossTexture = bossTex;
            this.gameManager = gm;
        }

        public void Load()
        {
            Enemies.Clear();
            elapsedTime = 0f;
            hasEnemiesSpawned = false; 

            System.Random rng = new System.Random();

            // LEVEL CONFIGURATIONS            
            // Level 1: Outer Orbit (Easy - Scouts Only)
            if (LevelNumber == 1)
            {
                for (int i = 0; i < 6; i++)
                {
                    float randomX = rng.Next(50, gameManager.ScreenWidth - 50);
                    // Spawn above screen so they fly in
                    float startY = -100 - (i * 150); 
                    Enemies.Add(new AlienScout(alienScoutTexture, new Vector2(randomX, startY), gameManager));
                }
            }
            // Level 2: Asteroid Belt (Medium - Mixed Enemies)
            else if (LevelNumber == 2)
            {
                for (int i = 0; i < 10; i++)
                {
                    float randomX = rng.Next(50, gameManager.ScreenWidth - 50);
                    float startY = -100 - (i * 120);

                    if (i % 2 == 0)
                        Enemies.Add(new AlienFighter(alienFighterTexture, new Vector2(randomX, startY), gameManager));
                    else
                        Enemies.Add(new AlienScout(alienScoutTexture, new Vector2(randomX, startY), gameManager));
                }
            }
            // Level 3: Alien Fleet (Hard - Heavy Density)
            else if (LevelNumber == 3)
            {
                for (int i = 0; i < 15; i++)
                {
                    float randomX = rng.Next(50, gameManager.ScreenWidth - 50);
                    float startY = -100 - (i * 100); 
                    
                    // 1 Scout for every 2 Fighters
                    if (i % 3 == 0) 
                        Enemies.Add(new AlienScout(alienScoutTexture, new Vector2(randomX, startY), gameManager));
                    else
                        Enemies.Add(new AlienFighter(alienFighterTexture, new Vector2(randomX, startY), gameManager));
                }
            }
            // Level 4: Boss Battle
            else if (LevelNumber == 4)
            {
                Enemies.Add(new BossAlienDragon(bossTexture, new Vector2(250, -200), gameManager));
            }

            // Flag that enemies are queued up
            if (Enemies.Count > 0)
            {
                hasEnemiesSpawned = true;
            }
        }

        public void Update(GameTime gameTime, Player player)
        {
            elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Brief delay at start of level before enemies begin logic
            if (elapsedTime >= spawnDelay)
            {
                foreach (var enemy in Enemies)
                    enemy.Update(gameTime, player);
            }

            // Cleanup destroyed enemies
            Enemies.RemoveAll(e => !e.IsActive);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var enemy in Enemies)
            {
                enemy.Draw(spriteBatch);
            }

            foreach (var pu in PowerUps)
            {
                pu.Draw(spriteBatch);
            }
        }
    }
}
