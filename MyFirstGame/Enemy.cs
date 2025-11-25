using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyFirstGame
{
    // Base Enemy Class
    public abstract class Enemy
    {
        protected GameManager gameManager;
        private static Random random = new Random();

        // Visuals & Stats
        public Texture2D Texture { get; protected set; }
        public string Name { get; private set; }
        public int HP { get; private set; }
        public bool IsActive { get; set; }
        
        // Position & Movement
        public Vector2 Position { get; protected set; }
        public Vector2 Size { get; protected set; }
        protected float speed;

        // Combat State
        protected float flashTimer = 0f; // For hit feedback
        protected List<Projectile> enemyProjectiles;
        protected float hitboxMargin = 0.15f; // Default 15% shrink for small enemies

        // Collision Hitbox
        public Rectangle BoundingBox
        {
            get
            {
                // Shrink hitbox slightly (15% margin) so shots must hit the "body" not empty corners
                int marginX = (int)(Size.X * 0.15f); 
                int marginY = (int)(Size.Y * 0.15f);

                return new Rectangle(
                    (int)Position.X + marginX,
                    (int)Position.Y + marginY,
                    (int)Size.X - (marginX * 2),
                    (int)Size.Y - (marginY * 2)
                );
            }
        }

        public Enemy(string name, int hp, float speed, Texture2D texture, Vector2 startPosition)
        {
            this.Name = name;
            this.HP = hp;
            this.speed = speed;
            this.Texture = texture;
            this.Position = startPosition;
            this.IsActive = true;
            this.Size = new Vector2(32, 32); 

            enemyProjectiles = new List<Projectile>();
        }

        public virtual void Update(GameTime gameTime, Player player)
        {
            // Default movement: drift downwards
            Position += new Vector2(0, speed);
            
            // Deactivate if it drifts too far off-screen
            if (Position.Y > 1000) IsActive = false;

            // Handle damage flash effect
            if (flashTimer > 0)
            {
                flashTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        public virtual void TakeDamage(int damage)
        {
            this.HP -= damage;
            flashTimer = 0.1f; // Flash red for 0.1s
            
            if (this.HP <= 0)
            {
                IsActive = false;
                
                // 10% chance to drop a powerup
                if (random.Next(10) == 1 && gameManager != null)
                {
                    gameManager.SpawnPowerUp(this.Position);
                }
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            Rectangle destinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);

            // Tint Red if taking damage
            Color drawColor = (flashTimer > 0) ? Color.Red : Color.White;

            spriteBatch.Draw(this.Texture, destinationRectangle, drawColor);
        }
    }
}