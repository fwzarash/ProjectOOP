using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyFirstGame
{
    public class Projectile
    {
        private int width;
        private int height; 

        public Texture2D Texture { get; private set; }
        public Vector2 Position { get; private set; }
        public int Damage { get; private set; }
        public string Owner { get; private set; }
        private float speed;
        public bool IsActive { get; set; }
        private Vector2 direction;

        // Expose direction and speed for external updates if needed
        public Vector2 Direction { get { return direction; } set { direction = value; } }
        public float Speed { get { return speed; } set { speed = value; } }
        private float scale ;

        public Rectangle BoundingBox
        {
            get
            {
                return new Rectangle(
                    (int)Position.X,
                    (int)Position.Y,
                    (int)(Texture.Width * scale), 
                    (int)(Texture.Height * scale)
                );
            }
        }

        public Projectile(Texture2D texture, Vector2 startPosition,
                          int damage, string owner,
                          Vector2 direction, float speed, float scale )
        {
            Texture = texture;
            Position = startPosition;
            Damage = damage;
            Owner = owner;
            this.speed = speed;
            this.scale = scale;

            // bullet uses real texture size
            width = texture.Width;
            height = texture.Height;

            if (direction != Vector2.Zero)
                direction.Normalize();

            this.direction = direction;
            IsActive = true;
        }

        public void Update(GameTime gameTime, GraphicsDevice graphicsDevice)
        {
            Position += direction * speed;

            // Deactivate if offscreen
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

            int w = Texture?.Width ?? 4;
            int h = Texture?.Height ?? 4;

            // Use rectangle drawing to maintain consistent size
            Rectangle rect = new Rectangle((int)Position.X, (int)Position.Y, width, height);
            // spriteBatch.Draw(Texture, rect, Color.White);
            // Change 1.0f to something smaller like 0.2f (20% size)

            spriteBatch.Draw(
                Texture, 
                Position, 
                null, 
                Color.White, 
                0f, 
                Vector2.Zero, 
                scale, // <--- THIS MAKES IT SMALLER
                SpriteEffects.None, 
                0f
            );
        }
    }
}
