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

        // --- FIX: Prevent instant skipping ---
        private bool hasEnemiesSpawned = false;
        public bool IsComplete 
        { 
            get { return hasEnemiesSpawned && Enemies.Count == 0; } 
        }
        // -------------------------------------

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
            gameManager = gm;
        }

        public void Load()
        {
            Enemies.Clear();
            elapsedTime = 0f;
            hasEnemiesSpawned = false; // Reset this flag

            System.Random rng = new System.Random();

            // ====================================================
            // LEVEL 1 - OUTER ORBIT (EASY)
            // ====================================================
            if (LevelNumber == 1)
            {
                for (int i = 0; i < 6; i++)
                {
                    float randomX = rng.Next(50, gameManager.ScreenWidth - 50);
                    float startY = -100 - (i * 150); 
                    
                    // FIX DI SINI: Tambah 'gameManager' di belakang
                    Enemies.Add(new AlienScout(alienScoutTexture, new Vector2(randomX, startY), gameManager));
                }
            }
            // ====================================================
            // LEVEL 2 - ASTEROID BELT (MEDIUM)
            // ====================================================
            else if (LevelNumber == 2)
            {
                for (int i = 0; i < 10; i++)
                {
                    float randomX = rng.Next(50, gameManager.ScreenWidth - 50);
                    float startY = -100 - (i * 120);

                    // 50% Fighters, 50% Scouts
                    if (i % 2 == 0)
                        Enemies.Add(new AlienFighter(alienFighterTexture, new Vector2(randomX, startY), gameManager));
                    else
                        // FIX DI SINI: Tambah 'gameManager' di belakang
                        Enemies.Add(new AlienScout(alienScoutTexture, new Vector2(randomX, startY), gameManager));
                }
            }
            // ====================================================
            // LEVEL 3 - ALIEN FLEET (HARD)
            // ====================================================
            else if (LevelNumber == 3)
            {
                for (int i = 0; i < 15; i++)
                {
                    float randomX = rng.Next(50, gameManager.ScreenWidth - 50);
                    float startY = -100 - (i * 100); 
                    
                    if (i % 3 == 0) 
                        // FIX DI SINI: Tambah 'gameManager' di belakang
                        Enemies.Add(new AlienScout(alienScoutTexture, new Vector2(randomX, startY), gameManager));
                    else
                        Enemies.Add(new AlienFighter(alienFighterTexture, new Vector2(randomX, startY), gameManager));
                }
            }
            // ====================================================
            // LEVEL 4 - FINAL BOSS
            // ====================================================
            else if (LevelNumber == 4)
            {
                // Boss tak perlu gameManager sebab dia belum ada logic tembak dalam kod awak
                Enemies.Add(new BossAlienDragon(bossTexture, new Vector2(250, -200)));
            }

            // Confirm that enemies have been added
            if (Enemies.Count > 0)
            {
                hasEnemiesSpawned = true;
            }
        }

        public void Update(GameTime gameTime, Player player)
        {
            elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Wait 1 second (spawnDelay) before letting enemies move
            if (elapsedTime >= spawnDelay)
            {
                foreach (var enemy in Enemies)
                    enemy.Update(gameTime, player);
            }

            // Remove dead enemies
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