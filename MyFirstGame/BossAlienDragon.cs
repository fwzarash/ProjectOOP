using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MyFirstGame
{
    public class BossAlienDragon : Enemy
    {
        private int phases;
        private string weakSpots;
        private double attackTimer;

        // Movement variables
        private float horizontalSpeed = 150f; // Pixels per second
        private bool movingRight = true;

        private float hoverHeight = 10f;

        public BossAlienDragon(Texture2D texture, Vector2 startPosition, GameManager gm)
            : base("Alien Dragon", 1000, 0.5f, texture, startPosition) // Increased HP for Boss
        {
            // Use full texture size for the Boss
            Size = new Vector2(texture.Width, texture.Height);
            this.gameManager = gm;

            this.hitboxMargin = 0.0f; // No hitbox margin for large boss
            
            phases = 3;
            weakSpots = "Glowing Core";
            attackTimer = 0;
        }

        public override void Update(GameTime gameTime, Player player)
        {
            base.Update(gameTime, player);
            
            // --- Entry Logic ---
            // Force the boss to slowly descend until it is fully on screen (Y = 50)
            if (Position.Y < hoverHeight)
            {
                float descentSpeed = 50f;
                Position = new Vector2(Position.X, Position.Y + descentSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds);
            }
            else
            {
                float moveAmount = horizontalSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                float newX = Position.X;

                if (movingRight)
                    newX += moveAmount;
                else
                    newX -= moveAmount;

                // Check Bounds using GameManager's screen width
                int screenWidth = gameManager.ScreenWidth;
                
                // Bounce off Left Wall
                if (newX <= 0)
                {
                    newX = 0;
                    movingRight = true; 
                }
                // Bounce off Right Wall
                else if (newX >= screenWidth - Size.X)
                {
                    newX = screenWidth - Size.X;
                    movingRight = false; 
                }

                Position = new Vector2(newX, hoverHeight);
            }

            // --- Attack Logic ---
            attackTimer += gameTime.ElapsedGameTime.TotalSeconds;

            if (attackTimer > 2.0)
            {
                // If HP is low (Final Phase), use Special Attack
                if (HP < 500) // Adjusted threshold for Special Attack
                    SpecialAttack(player);
                else
                {
                    RegularAttack(player);
                }
                // Reset timer (make it faster if enraged)
                attackTimer = (phases < 3) ? 0.5 : 0;           
            }
        }

        private void RegularAttack(Player player)
        {
            // Logic: Calculate direction vector towards player
            Vector2 origin = new Vector2(Position.X + Size.X / 2, Position.Y + Size.Y / 2 - 20);
            Vector2 target = player.Position + (player.Size / 2);
            Vector2 direction = target - origin;
            
            if (direction != Vector2.Zero) direction.Normalize();

            Projectile p = new Projectile(
                gameManager.EnemyProjectileTexture,
                origin,
                15,                 // Higher damage than normal enemies
                Name,
                direction,          // Aimed at player
                7f,                 // Fast speed
                1.5f                // Larger bullet
            );

            gameManager.AddEnemyProjectile(p);
        }

        public void SpecialAttack(Player player)
        {
            // Logic: "Ring of Death" - Spawn 12 bullets in a circle
            Vector2 origin = new Vector2(Position.X + Size.X / 2, Position.Y + Size.Y / 2);
            int numberOfBullets = 12;

            for (int i = 0; i < numberOfBullets; i++)
            {
                // Calculate angle for this specific bullet
                float angle = i * (MathHelper.TwoPi / numberOfBullets);
                
                // Convert angle to Vector2 direction
                Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

                Projectile p = new Projectile(
                    gameManager.EnemyProjectileTexture,
                    origin,
                    10,
                    Name,
                    direction,
                    5f,
                    1.0f
                );

                gameManager.AddEnemyProjectile(p);
            }
        }

        public void ChangePhase()
        {
            phases--;
            horizontalSpeed *= 1.5f;
            System.Diagnostics.Debug.WriteLine($"{Name} is enraged! Target its {weakSpots}!");
        }

        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);

            // Trigger phases based on HP thresholds
            if (phases == 3 && HP <= 500)
                ChangePhase();
            else if (phases == 2 && HP <= 200)
                ChangePhase();
        }
    }
}