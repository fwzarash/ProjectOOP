using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyFirstGame
{
    public class AlienScout : Enemy
    {
        // Tambah reference GameManager untuk spawn peluru
        // private GameManager gameManager;
        private float shootTimer = 0f;
        
        // Scout tembak lambat sikit dari Fighter
        private float shootInterval = 3.0f; 

        // RANDOM GENERATOR (Static so all scouts share the same randomizer logic)
        public static Random rng = new Random();

        // UNIQUE MOVEMENT VARIABLES
        private float waveFrequency; // How fast it wiggles
        private float waveAmplitude; // How wide it wiggles (Speed left/right)

        // Tambah 'GameManager gm' dalam constructor
        public AlienScout(Texture2D texture, Vector2 startPosition, GameManager gm) 
            : base("Alien Scout", 50, 1.0f, texture, startPosition)
        {
            // Set collision size (scaled)
            this.Size = new Vector2(texture.Width * 0.3f, texture.Height * 0.3f);
            this.gameManager = gm; // Simpan game manager

            // RANDOMIZE THE ZIG-ZAG PATTERN
            // Frequency between 1.0 and 4.0
            this.waveFrequency = (float)rng.NextDouble() * 3f + 1f; 
            
            // Amplitude (Speed) between 1.0 and 3.0
            this.waveAmplitude = (float)rng.NextDouble() * 2f + 1f;
        }

        public override void Update(GameTime gameTime, Player player)
        {
            base.Update(gameTime, player); // <--- This ensures the timer counts down!
            // Gerakan Zig-Zag (Sine Wave)
            float time = (float)gameTime.TotalGameTime.TotalSeconds;
            float xOffset = (float)Math.Sin(time * waveFrequency) * waveAmplitude;
            // Calculate potential new X position
            float newX = Position.X + xOffset;

            // 5. KEEP INSIDE WINDOW (CLAMPING)
            // Assuming Screen Width is 800. If your game is wider, change 800.
            // We clamp between 0 and (800 - EnemyWidth) so it doesn't clip the right side.
            newX = MathHelper.Clamp(newX, 0, gameManager.ScreenWidth - Size.X);

            Position = new Vector2(newX, Position.Y + speed);

            // LOGIC MENEMBAK (NEW)
            shootTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (shootTimer >= shootInterval)
            {
                shootTimer = 0f;
                Shoot();
            }

            // Use gameManager to get the REAL screen height
            if (Position.Y > gameManager.ScreenHeight) IsActive = false;
        }

        private void Shoot()
        {
            // Tembak peluru merah ke bawah
            Vector2 bulletPos = new Vector2(Position.X + Size.X / 2, Position.Y +50);
            
            Projectile p = new Projectile(
                gameManager.EnemyProjectileTexture, // Guna texture merah dari GM
                bulletPos,
                5, // Damage kecil
                Name,
                new Vector2(0, 1), // Arah Bawah
                5f, // Speed peluru
                1.0f
            );
            
            gameManager.AddEnemyProjectile(p);
        }
    }
}