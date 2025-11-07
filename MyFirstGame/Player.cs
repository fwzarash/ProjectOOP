using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MyFirstGame
{
    public class Player
    {
        public Texture2D Texture { get; private set; }
        public Vector2 Position { get; private set; }
        private Texture2D projectileTexture;
        private float projectileSpeed;
        private float shootCooldown;
        public Vector2 Size { get; private set; }

        private int hp;
        private float speed;
        private float fireRate;
        private int weaponLevel;
        public int WeaponLevel { get { return weaponLevel; } }

        public string Name { get; private set; }
        public int HP { get { return hp; } }

        public Player(string name, Texture2D texture, Texture2D projectileTexture, Vector2 startPosition)
        {
            this.Name = name;
            this.Texture = texture;
            this.Position = startPosition;

            // Set default values from your design
            this.hp = 100;
            this.speed = 4f; // This will now mean "pixels per frame"
            this.fireRate = 0.5f;
            this.weaponLevel = 1;
            this.shootCooldown = 0f; // Player can shoot immediately

            this.projectileTexture = projectileTexture;
            this.projectileSpeed = 10.0f;
            this.Size = new Vector2(32, 32); // A 32x32 pixel square
        }

        public Rectangle BoundingBox
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
            }
        }

        public void Update(GameTime gameTime)
        {
            if (shootCooldown > 0)
            {
                shootCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            KeyboardState kState = Keyboard.GetState();
            Vector2 direction = Vector2.Zero;

            if (kState.IsKeyDown(Keys.Left) || kState.IsKeyDown(Keys.A))
            {
                direction.X = -1;
            }
            if (kState.IsKeyDown(Keys.Right) || kState.IsKeyDown(Keys.D))
            {
                direction.X = 1;
            }

            Position += direction * speed;
            
            if (kState.IsKeyDown(Keys.Space) && shootCooldown <= 0)
            {
                shootCooldown = fireRate; 
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle destinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
            spriteBatch.Draw(
                this.Texture,
                this.Position,
                // destinationRectangle,
                Color.White
            );
        }

        public Projectile Shoot()
        {
            Vector2 spawnPosition = new Vector2(
                this.Position.X + (this.Texture.Width / 2) - (this.projectileTexture.Width / 2),
                this.Position.Y
            );

            Vector2 direction = new Vector2(0, -1);

            return new Projectile(
                this.projectileTexture,
                spawnPosition,
                10 * weaponLevel,
                this.Name,
                direction,                  
                this.projectileSpeed
            );
        }

        public void TakeDamage(int damage)
        {
            this.hp -= damage;
            if (this.hp <= 0)
            {
                // Set a flag, e.g., IsAlive = false
            }
        }

        public void AddHealth(int value)
        {
            this.hp += value;
        }

        public void UpgradeWeapon(int value)
        {
            this.weaponLevel += value;
        }
    }
}