using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyFirstGame
{
    public abstract class Enemy
    {
        public Texture2D Texture { get; protected set; }
        public Vector2 Position { get; protected set; }

        // Core attributes from your design
        protected string name;
        protected int hp;
        protected float speed;
        public Vector2 Size { get; protected set; }

        // Public accessors
        public string Name { get { return name; } }
        public int HP { get { return hp; } }
        public bool IsActive { get; set; }

        // Bounding box for collision detection
        public Rectangle BoundingBox
        {
            get
            {
                // From: return new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
                // To:
                return new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
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
        }

        // Abstract method for AI and movement
        // Replaces the original 'Move()' and 'Attack()'
        public abstract void Update(GameTime gameTime, Player player);

        // Virtual method for taking damage
        public virtual void TakeDamage(int damage)
        {
            this.hp -= damage;
            if (this.hp <= 0)
            {
                this.IsActive = false;
                // Add logic here for score, explosions, etc.
            }
        }

        // Virtual method for drawing (most enemies draw the same way)
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            Rectangle destinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
            spriteBatch.Draw(
                this.Texture,
                destinationRectangle,
                Color.Red // Enemies will be red squares
            );
        }
    }
}