using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyFirstGame
{
    public class AlienFighter : Enemy
    {
        private Texture2D projectileTexture;
        private float shootCooldown;
        private float fireRate = 2f; // Shoot every 2 seconds
        // private GameManager gameManager;

        public AlienFighter(Texture2D texture, Vector2 startPosition, GameManager gm)
            : base("Alien Fighter", 80, 1.0f, texture, startPosition) // Name, HP, speed
        {
            // Set collision size (scaled)
            this.Size = new Vector2(texture.Width * 0.3f, texture.Height * 0.3f);

            this.projectileTexture = gm.EnemyProjectileTexture;
            this.shootCooldown = fireRate;
            this.gameManager = gm;
        }

        public override void Update(GameTime gameTime, Player player)
        {
            base.Update(gameTime, player); // <--- This ensures the timer counts down!
            // Move down
            float newX = MathHelper.Clamp(Position.X, 0, 800 - Size.X);
            Position = new Vector2(newX, Position.Y + speed);

            // Shooting logic
            shootCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (shootCooldown <= 0f)
            {
                shootCooldown = fireRate;

                Vector2 spawnPos = new Vector2(
                    Position.X + Size.X / 2 - projectileTexture.Width / 2,
                    Position.Y - 20
                );

                Projectile p = new Projectile(
                    projectileTexture,
                    spawnPos,
                    5,                    // damage
                    Name,
                    new Vector2(0, 1),    // downward direction
                    6f,                    // speed
                    0.8f
                );

                gameManager.AddEnemyProjectile(p);
            }

            // Use gameManager to get the REAL screen height
            if (Position.Y > gameManager.ScreenHeight) IsActive = false;
        }
    }
}
