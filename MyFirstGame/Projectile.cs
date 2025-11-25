using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyFirstGame
{
    public class Projectile
    {
        // Visuals
        public Texture2D Texture { get; private set; }
        private int width;
        private int height; 
        private float scale;

        // Position & Movement
        public Vector2 Position { get; private set; }
        public Vector2 Direction { get; set; }
        public float Speed { get; set; }

        // Game Logic
        public int Damage { get; private set; }
        public string Owner { get; private set; } // "Player" or Enemy Name
        public bool IsActive { get; set; }

        public Rectangle BoundingBox
        {
            get
            {
                // Calculate scaled hit box
                return new Rectangle(
                    (int)Position.X,
                    (int)Position.Y,
                    (int)(Texture.Width * scale), 
                    (int)(Texture.Height * scale)
                );
            }
        }

        public Projectile(Texture2D texture, Vector2 startPosition, int damage, string owner, Vector2 direction, float speed, float scale)
        {
            this.Texture = texture;
            this.Position = startPosition;
            this.Damage = damage;
            this.Owner = owner;
            this.Speed = speed;
            this.scale = scale;
            this.width = texture.Width;
            this.height = texture.Height;

            // Normalize direction to ensure consistent speed regardless of angle
            if (direction != Vector2.Zero)
                direction.Normalize();

            this.Direction = direction;
            this.IsActive = true;
        }

        public void Update(GameTime gameTime, GraphicsDevice graphicsDevice)
        {
            Position += Direction * Speed;

            // Deactivate if it leaves the screen
            if (Position.Y < -height || Position.Y > graphicsDevice.Viewport.Height)
                IsActive = false;
        }

        public void OnHit()
        {
            IsActive = false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsActive) return;

            spriteBatch.Draw(
                Texture, 
                Position, 
                null, 
                Color.White, 
                0f, 
                Vector2.Zero, 
                scale, // Draw using the defined scale
                SpriteEffects.None, 
                0f
            );
        }
    }
}