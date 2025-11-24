using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;    

namespace MyFirstGame
{
    public class Player
    {
        public Texture2D Texture { get; private set; }

        private Vector2 position;
        public Vector2 Position { get { return position; } }

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

        // task mia
        private bool isInvincible;
        private float invincibilityTimer = 0f;
        private const float INVINCIBILITY_DURATION = 1.0f; // 1 second of invulnerability

        private float flashTimer;
        private const float FLASH_INTERVAL = 0.1f; // Flash every 0.1 seconds
        private bool isVisible; 

        public bool HasShield   {get ; private set; } 
        // 

        //mus
        private SoundEffect _reloadSfx;


        public bool IsShooting { get; private set; } = false;

        // =============================================================
        // (NEW) System Ammo & Reload
        // =============================================================
        public int MaxAmmo { get; private set; } = 30; // Limit peluru
        public int CurrentAmmo { get; private set; }
        // =============================================================

        // Invincibility vars
        // private float invincibilityTimer = 0f;
        private Color playerColor = Color.White;

        public Player(string name, Texture2D texture, Texture2D projectileTexture, Vector2 startPosition, SoundEffect reloadSfx)
        {
            this.Name = name;
            this.Texture = texture;
            this.position = startPosition;

            this.hp = 100;
            this.speed = 4f; 
            this.fireRate = 0.75f;
            this.weaponLevel = 1;
            this.shootCooldown = 0f;

            _reloadSfx = reloadSfx;

            // (NEW) Initialize Ammo penuh
            this.CurrentAmmo = MaxAmmo; 

            this.projectileTexture = projectileTexture;
            this.projectileSpeed = 10.0f;

            // this.Size = new Vector2(texture.Width * 0.3f, texture.Height * 0.3f); 
            this.Size = new Vector2(128, 128);

            this.isInvincible = false;
            this.invincibilityTimer = 0f;
            this.flashTimer = 0f;
            this.isVisible = true;
            this.HasShield = false; // Player starts of with no shield
        }

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
            if (shootCooldown > 0)
            {
                // shootCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                shootCooldown -= deltaTime ;
            }

            if (isInvincible)
            {
                invincibilityTimer -=  deltaTime ; 
                flashTimer -= deltaTime ;

                // Handles flashing effect 
                if (flashTimer <= 0)
                {
                    // Toggle visibility every flash interval
                    isVisible = !isVisible;
                    flashTimer = FLASH_INTERVAL ;

                }

                // End invincibility 
                if (invincibilityTimer <= 0)
                {
                    isInvincible = false ;
                    // Make player visible fully when invincibility wears off 
                    isVisible = true ; 
                }
            }

            KeyboardState kState = Keyboard.GetState();
            Vector2 direction = Vector2.Zero;

            // Movement
            if (kState.IsKeyDown(Keys.Left) || kState.IsKeyDown(Keys.A))
                direction.X = -1;
            if (kState.IsKeyDown(Keys.Right) || kState.IsKeyDown(Keys.D))
                direction.X = 1;

            position += direction * speed;

            // if (kState.IsKeyDown(Keys.Space) && shootCooldown <= 0)
            // {
            //     shootCooldown = fireRate; 
            // }

            position.X = MathHelper.Clamp(position.X, 0, screenBounds.Width - Size.X);
            position.Y = MathHelper.Clamp(position.Y, 0, screenBounds.Height - Size.Y);

            // Cooldown
            if (shootCooldown > 0)
                shootCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            // =============================================================
            // (NEW) Logic Reload (Tekan R)
            // =============================================================
            if (kState.IsKeyDown(Keys.R) && CurrentAmmo < MaxAmmo)
            {
                CurrentAmmo = MaxAmmo; // Isi penuh balik peluru
                if (_reloadSfx != null) 
            {
                _reloadSfx.Play(1.0f, 0.5f, 0.0f);
            }
            }
            // =============================================================

            // Invincibility Logic
            if (invincibilityTimer > 0)
            {
                invincibilityTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                
                // Efek berkelip
                if ((invincibilityTimer * 20) % 2 > 1) 
                    playerColor = Color.Red;
                else 
                    playerColor = Color.White;
            }
            else
            {
                playerColor = Color.White;
            }
        }

        // (NEW) Check Ammo > 0 sebelum menembak
        public bool CanShoot() => shootCooldown <= 0 && CurrentAmmo > 0;

        public void ResetCooldown() => shootCooldown = fireRate;

        public void Draw(SpriteBatch spriteBatch)
        {
            if (isVisible)
            {
                Rectangle destinationRectangle = new Rectangle(
                    (int)position.X,
                    (int)position.Y,
                    (int)Size.X, 
                    (int)Size.Y
                );
                spriteBatch.Draw(Texture, destinationRectangle, playerColor);
            }
            if (HasShield && isVisible) 
            {
                Rectangle destinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
                spriteBatch.Draw(
                    this.Texture,
                    destinationRectangle,
                    Color.Blue * 0.5f // 50% transparent blue 
                );
            }
            
        }

// Dalam Player.cs

        public Projectile Shoot()
        {
            // 1. DEFINE THE SCALE
            // IMPORTANT: This number MUST match the scale you used in Projectile.cs!
            // If you used 0.15f there, use 0.15f here.
            float bulletScale = 0.15f; 

            // 2. CALCULATE THE REAL WIDTH
            float realBulletWidth = this.projectileTexture.Width * bulletScale;

            // 3. CENTER IT BASED ON THE REAL WIDTH
            float startX = this.Position.X + (this.Size.X / 2) - (realBulletWidth / 2);
            
            // 4. SPAWN HEIGHT (Adjust +20 if needed)
            float startY = this.Position.Y + 20;
            // // We use 'Size.X' (128) instead of 'Texture.Width' to find the visual center.
            // float startX = this.Position.X + (this.Size.X / 2) - (this.projectileTexture.Width / 2);
            
            // // We subtract the bullet height so it spawns slightly ABOVE the ship, not inside it.
            // float startY = this.Position.Y + 20;

            Vector2 spawnPosition = new Vector2(startX, startY);
            // Vector2 spawnPosition = new Vector2(
            //     this.Position.X + (this.Texture.Width / 2) - (this.projectileTexture.Width / 2),
            //     this.Position.Y
            // );
            Vector2 direction = new Vector2(0, -1);
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

            // 1. Tolak peluru
            

            // 2. POSISI SPAWN (PENTING)
            // Formula: Ambil tengah player (X), dan ambil atas kepala player (Y - 20)
            // float spawnX = position.X + (Size.X / 2) - (projectileTexture.Width / 2);
            // float spawnY = position.Y - 20; // Tolak 20 supaya muncul DI ATAS kepala, bukan dalam badan

            // Vector2 spawnPosition = new Vector2(spawnX, spawnY);

            // // 3. ARAH (DIRECTION)
            // // X = 0 (Tak gerak kiri kanan)
            // // Y = -1 (NAIK ATAS) 
            // // JANGAN GUNA (0, 1) -> Itu turun bawah!
            // Vector2 direction = new Vector2(0, -1); 

            // // 4. Create Bullet
            // return new Projectile(
            //     projectileTexture,
            //     spawnPosition,
            //     10 * weaponLevel, // Damage
            //     Name,
            //     direction,        // Arah yang dah dibetulkan
            //     projectileSpeed
            // );
        }

        public void TakeDamage(int damage)
        {
            if (HasShield)
            {
                HasShield = false;
                return;
            }

            //Check for invulnerability, proceed if no shield and not already invincible
            if (isInvincible)
            {
                return ;
            } 

            this.hp -= damage;
            isInvincible = true;
            invincibilityTimer = INVINCIBILITY_DURATION ;
            flashTimer = FLASH_INTERVAL ; 

            if (this.hp <= 0)
            {
                // Logic mati
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