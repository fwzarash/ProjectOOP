using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MyFirstGame
{
    public class AlienFighter : Enemy
    {
        private Texture2D projectileTexture;
        private float shootCooldown;
        private float fireRate = 2f; // Shoot every 2 seconds

        // Random generator for any future randomization needs
        private static Random rng = new Random();

        public AlienFighter(Texture2D texture, Vector2 startPosition, GameManager gm)
            : base("Alien Fighter", 80, 1.0f, texture, startPosition)
        {
            this.Size = new Vector2(texture.Width * 0.3f, texture.Height * 0.3f);
            this.gameManager = gm;
            this.projectileTexture = gm.EnemyProjectileTexture;
            this.shootCooldown = (float)rng.NextDouble() * fireRate;
        }

        public override void Update(GameTime gameTime, Player player)
        {
            base.Update(gameTime, player);

            // Restrict horizontal movement to screen bounds
            float newX = MathHelper.Clamp(Position.X, 0, 800 - Size.X);
            Position = new Vector2(newX, Position.Y + speed);

            // Shooting Logic
            shootCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (shootCooldown <= 0f)
            {
                shootCooldown = fireRate;
                Shoot();
            }

            if (Position.Y > gameManager.ScreenHeight) IsActive = false;
        }

        private void Shoot()
        {
            Vector2 spawnPos = new Vector2(
                Position.X + Size.X / 2 - projectileTexture.Width / 2,
                Position.Y + Size.Y - 50
            );

            Projectile p = new Projectile(
                projectileTexture,
                spawnPos,
                5,                    // Damage
                Name,
                new Vector2(0, 1),    // Downward direction
                6f,                   // Speed
                0.8f                  // Scale
            );

            gameManager.AddEnemyProjectile(p);
        }
    }
}