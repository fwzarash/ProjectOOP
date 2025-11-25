using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyFirstGame
{
    public class AlienScout : Enemy
    {
        private float shootTimer = 0f;
        private float shootInterval = 3.0f; // Slower fire rate than Fighter
        
        public static Random rng = new Random();

        // Variables for Zig-Zag movement
        private float waveFrequency; 
        private float waveAmplitude; 

        public AlienScout(Texture2D texture, Vector2 startPosition, GameManager gm) 
            : base("Alien Scout", 50, 1.0f, texture, startPosition)
        {
            this.Size = new Vector2(texture.Width * 0.3f, texture.Height * 0.3f);
            this.gameManager = gm; 

            // Randomize flight path patterns
            this.waveFrequency = (float)rng.NextDouble() * 3f + 1f; 
            this.waveAmplitude = (float)rng.NextDouble() * 2f + 1f;

            // Initialize shoot timer to a random offset so not all Scouts shoot simultaneously
            this.shootTimer = (float)rng.NextDouble() * shootInterval;
        }

        public override void Update(GameTime gameTime, Player player)
        {
            base.Update(gameTime, player); 
            
            // Zig-Zag Movement (Sine Wave)
            float time = (float)gameTime.TotalGameTime.TotalSeconds;
            float xOffset = (float)Math.Sin(time * waveFrequency) * waveAmplitude;
            float newX = Position.X + xOffset;

            // Clamp to screen width
            newX = MathHelper.Clamp(newX, 0, gameManager.ScreenWidth - Size.X);
            Position = new Vector2(newX, Position.Y + speed);

            // Shooting Logic
            shootTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (shootTimer >= shootInterval)
            {
                shootTimer = 0f;
                Shoot();
            }

            if (Position.Y > gameManager.ScreenHeight) IsActive = false;
        }

        private void Shoot()
        {
            Vector2 bulletPos = new Vector2(Position.X + Size.X / 2, Position.Y + Size.Y - 50);
            
            Projectile p = new Projectile(
                gameManager.EnemyProjectileTexture, 
                bulletPos,
                5, 
                Name,
                new Vector2(0, 1), // Down
                5f, 
                1.0f
            );
            
            gameManager.AddEnemyProjectile(p);
        }
    }
}