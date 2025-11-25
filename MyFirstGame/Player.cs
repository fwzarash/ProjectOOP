using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;    

namespace MyFirstGame
{
    public class Player
    {
        // Visuals & Position
        public Texture2D Texture { get; private set; }
        public string Name { get; private set; }
        private Vector2 position;
        public Vector2 Position { get { return position; } }
        public Vector2 Size { get; private set; }
        private Color playerColor = Color.White;

        // Stats & Combat
        private int hp;
        public int HP { get { return hp; } }
        private float speed;
        
        // Weapon System
        private Texture2D projectileTexture;
        private float projectileSpeed;
        private float shootCooldown;
        private float fireRate;
        private int weaponLevel;
        public int WeaponLevel { get { return weaponLevel; } }
        public bool IsShooting { get; private set; } = false;

        // Ammo System
        public int MaxAmmo { get; private set; } = 30; 
        public int CurrentAmmo { get; private set; }
        private SoundEffect _reloadSfx;

        // Invincibility & Shield
        private bool isInvincible;
        private float invincibilityTimer = 0f;
        private const float INVINCIBILITY_DURATION = 1.0f; // Duration of invulnerability after hit
        private float flashTimer;
        private const float FLASH_INTERVAL = 0.1f; // Flash frequency
        private bool isVisible; 
        public bool HasShield { get; private set; } 

        public Player(string name, Texture2D texture, Texture2D projectileTexture, Vector2 startPosition, SoundEffect reloadSfx)
        {
            this.Name = name;
            this.Texture = texture;
            this.position = startPosition;
            this.projectileTexture = projectileTexture;
            this._reloadSfx = reloadSfx;

            // Initialize Stats
            this.hp = 100;
            this.speed = 4f; 
            this.fireRate = 0.25f;
            this.weaponLevel = 1;
            this.shootCooldown = 0f;
            this.projectileSpeed = 10.0f;
            this.CurrentAmmo = MaxAmmo; 

            // Initialize Dimensions (Hardcoded for gameplay feel)
            this.Size = new Vector2(128, 128);

            // Initialize States
            this.isInvincible = false;
            this.invincibilityTimer = 0f;
            this.flashTimer = 0f;
            this.isVisible = true;
            this.HasShield = false; 
        }

        // Collision Hitbox
        public Rectangle BoundingBox
        {
            get
            {
                return new Rectangle(
                    (int)position.X,
                    (int)position.Y,
                    (int)Size.X,
                    (int)Size.Y
                );
            }
        }

        public void Update(GameTime gameTime, Rectangle screenBounds)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Cooldown Management
            if (shootCooldown > 0)
            {
                shootCooldown -= deltaTime;
            }

            // Invincibility Logic
            if (isInvincible)
            {
                invincibilityTimer -= deltaTime; 
                flashTimer -= deltaTime;

                // Handle flashing effect 
                if (flashTimer <= 0)
                {
                    isVisible = !isVisible;
                    flashTimer = FLASH_INTERVAL;
                }

                // End invincibility 
                if (invincibilityTimer <= 0)
                {
                    isInvincible = false;
                    isVisible = true; 
                    playerColor = Color.White;
                }
                else
                {
                    // Tint red slightly while invincible
                    if ((invincibilityTimer * 20) % 2 > 1) 
                        playerColor = Color.Red;
                    else 
                        playerColor = Color.White;
                }
            }

            // Movement Logic
            KeyboardState kState = Keyboard.GetState();
            Vector2 direction = Vector2.Zero;

            if (kState.IsKeyDown(Keys.Left) || kState.IsKeyDown(Keys.A))
                direction.X = -1;
            if (kState.IsKeyDown(Keys.Right) || kState.IsKeyDown(Keys.D))
                direction.X = 1;

            position += direction * speed;

            // Keep player within screen bounds
            position.X = MathHelper.Clamp(position.X, 0, screenBounds.Width - Size.X);
            position.Y = MathHelper.Clamp(position.Y, 0, screenBounds.Height - Size.Y);

            // Reload Logic
            if (kState.IsKeyDown(Keys.R) && CurrentAmmo < MaxAmmo)
            {
                CurrentAmmo = MaxAmmo; 
                if (_reloadSfx != null) 
                {
                    _reloadSfx.Play(1.0f, 0.5f, 0.0f);
                }
            }
        }

        public void ActivateShield()
        {
            this.HasShield = true;
        }

        public bool CanShoot() => shootCooldown <= 0 && CurrentAmmo > 0;

        public void ResetCooldown() => shootCooldown = fireRate;

        public Projectile Shoot()
        {
            // Calculate bullet spawn position
            float bulletScale = 0.15f; 
            float realBulletWidth = this.projectileTexture.Width * bulletScale;

            // Center the bullet horizontally relative to the ship
            float startX = this.Position.X + (this.Size.X / 2) - (realBulletWidth / 2);
            float startY = this.Position.Y + 20; // Slight offset to spawn near the nose of the ship

            Vector2 spawnPosition = new Vector2(startX, startY);
            Vector2 direction = new Vector2(0, -1); // Upwards

            CurrentAmmo--;

            return new Projectile(
                this.projectileTexture,
                spawnPosition,
                10 * weaponLevel,
                this.Name,
                direction,                  
                this.projectileSpeed,
                bulletScale
            );
        }

        public void TakeDamage(int damage)
        {
            // Shield absorbs damage completely once
            if (HasShield)
            {
                HasShield = false;
                return;
            }

            // Ignore damage if already invincible
            if (isInvincible) return;

            this.hp -= damage;
            
            // Trigger temporary invincibility
            isInvincible = true;
            invincibilityTimer = INVINCIBILITY_DURATION;
            flashTimer = FLASH_INTERVAL; 
        }

        public void AddHealth(int value)
        {
            this.hp += value;
        }

        public void UpgradeWeapon(int value)
        {
            this.weaponLevel += value;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (isVisible)
            {
                Rectangle destinationRectangle = new Rectangle(
                    (int)position.X, (int)position.Y, (int)Size.X, (int)Size.Y
                );
                spriteBatch.Draw(Texture, destinationRectangle, playerColor);
            }

            // Draw Shield Overlay
            if (HasShield && isVisible) 
            {
                Rectangle shieldRect = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
                spriteBatch.Draw(Texture, shieldRect, Color.Blue * 0.5f);
            }
        }
    }
}