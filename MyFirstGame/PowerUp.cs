using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyFirstGame
{
    public enum PowerUpType
    {
        Health,
        Shield,
        WeaponUpgrade
    }

    public class PowerUp
    {
        public Texture2D Texture { get; private set; }
        public Vector2 Position { get; private set; }
        public bool IsActive { get; set; }

        private PowerUpType type;
        private int value;
        private float speed = 2.0f; // Falling speed

        public Rectangle BoundingBox
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
            }
        }

        public PowerUp(Texture2D texture, Vector2 position, PowerUpType type, int value)
        {
            this.Texture = texture;
            this.Position = position;
            this.type = type;
            this.value = value;
            this.IsActive = true;
        }
        
        public void Update(GameTime gameTime)
        {
            Position = new Vector2(Position.X, Position.Y + speed);
        }

        public void Apply(Player player)
        {
            switch (type)
            {
                case PowerUpType.Health:
                    player.AddHealth(value);
                    break;
                case PowerUpType.WeaponUpgrade:
                    player.UpgradeWeapon(value);
                    break;
                case PowerUpType.Shield:
                    player.ActivateShield();
                    break;
            }
            this.IsActive = false; // Destroy after collection
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Tint based on type for visual distinction
            Color colorTint = Color.White;
            
            switch (type)
            {
                case PowerUpType.Health:
                    colorTint = Color.LimeGreen;
                    break;
                case PowerUpType.WeaponUpgrade:
                    colorTint = Color.Gold;
                    break;
                case PowerUpType.Shield:
                    colorTint = Color.Blue;
                    break;
            }

            spriteBatch.Draw(Texture, Position, colorTint);
        }
    }
}