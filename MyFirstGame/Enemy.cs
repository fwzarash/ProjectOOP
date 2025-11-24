using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyFirstGame
{
    // ==========================================
    // BASE ENEMY CLASS
    // ==========================================
    public abstract class Enemy
    {
        protected GameManager gameManager;
        private static Random random = new Random();

        public Texture2D Texture { get; protected set; }

        // Public properties
        protected string name { get; private set; }
        protected int hp { get; private set; }

        // Add a timer for the red flash
        protected float flashTimer = 0f;

        // public bool IsActive { get; protected set; } = true;

        // Position and size for collision/drawing
        public Vector2 Position { get; protected set; }
        public Vector2 Size { get; protected set; }

        // Protected speed field for derived classes
        protected float speed;

        // Texture for drawing
        // public Texture2D Texture { get; private set; }

         // Public accessors
        public string Name { get { return name; } }
        public int HP { get { return hp; } }
        public bool IsActive { get; set; }

        protected List<Projectile> enemyProjectiles;

        // Bounding box for collisions
        public Rectangle BoundingBox
        {
            get
            {
                // 1. Calculate a margin (e.g., 15% of the size)
                int marginX = (int)(Size.X * 0.15f); 
                int marginY = (int)(Size.Y * 0.15f);

                // 2. Create a rectangle that is smaller than the visual image
                return new Rectangle(
                    (int)Position.X + marginX,         // Move box RIGHT slightly
                    (int)Position.Y + marginY,         // Move box DOWN slightly
                    (int)Size.X - (marginX * 2),       // Make width smaller
                    (int)Size.Y - (marginY * 2)        // Make height smaller
                );
            }
        }
        // Constructor
        public Enemy(string name, int hp, float speed, Texture2D texture, Vector2 startPosition)
        {
            this.name = name;
            this.hp = hp;
            this.speed = speed;
            this.Texture = texture;
            this.Position = startPosition;
            this.IsActive = true;
            this.Size = new Vector2(32, 32); // Default 32x32 square

            enemyProjectiles = new List<Projectile>();

            // Default size (scale by 0.3f)
            // if (texture != null)
            //     this.Size = new Vector2(texture.Width * 0.3f, texture.Height * 0.3f);
            // else
            //     this.Size = new Vector2(32, 32);
        }

        // Update method (must be overridden for movement/AI)
        public virtual void Update(GameTime gameTime, Player player)
        {
            // Default movement: move straight down
            Position += new Vector2(0, speed);
            
            // Kill if way off screen
            if (Position.Y > 1000) IsActive = false;

            // Count down the timer
            if (flashTimer > 0)
            {
                flashTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        // Take damage method
        public virtual void TakeDamage(int damage)
        {
            this.hp -= damage;
            // Trigger the flash when hit
            flashTimer = 0.1f; // Flash red for 0.1 seconds
            if (this.hp <= 0)
            {
                IsActive = false;
                // Add logic here for score, explosions, etc.
                int randomNumber = random.Next(10);
                if (randomNumber == 1)
                {
                    gameManager.SpawnPowerUp(this.Position);
                }
            }
        }

        // Draw the enemy
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            Rectangle destinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);

            // Decide color based on timer
            Color drawColor = Color.White;
            if (flashTimer > 0)
            {
                drawColor = Color.Red; // Draw RED if timer is active
            }

            spriteBatch.Draw(
                this.Texture,
                destinationRectangle,
                drawColor // Enemies will be red squares
            );
            // if (Texture != null)
            //     spriteBatch.Draw(Texture, new Rectangle(Position.ToPoint(), Size.ToPoint()), Color.White);
        }
    }
}