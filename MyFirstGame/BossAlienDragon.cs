using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyFirstGame
{
    public class BossAlienDragon : Enemy
    {
        private int phases;
        private string weakSpots;
        private double attackTimer;

        public BossAlienDragon(Texture2D texture, Vector2 startPosition)
            : base("Alien Dragon", 500, 0.5f, texture, startPosition)
        {
            // UBAH: Jangan kecilkan saiz (buang * 0.3f) supaya Boss nampak besar
            Size = new Vector2(texture.Width, texture.Height);
            
            phases = 3;
            weakSpots = "Glowing Core";
            attackTimer = 0;
        }

        public override void Update(GameTime gameTime, Player player)
        {
            base.Update(gameTime, player);
            // --- PEMBETULAN PENTING: LOGIC MASUK SKRIN ---
            // Kalau Boss masih di bahagian atas skrin (Y < 50), suruh dia turun
            if (Position.Y < 50)
            {
                Position = new Vector2(Position.X, Position.Y + 1.0f); // Turun ke bawah perlahan-lahan
            }
            else
            {
                // Bila dah masuk skrin, baru gerak Kiri-Kanan (Sine Wave)
                // Position = new Vector2(Position.X + (float)System.Math.Sin(gameTime.TotalGameTime.TotalSeconds) * 2, Position.Y);
                // We FORCE Position.Y to stay at 50 (or whatever height you want)
                // so he never drifts down further.
                float hoverY = 50; 
                
                // Sine wave for Left/Right movement
                float swayOffset = (float)System.Math.Sin(gameTime.TotalGameTime.TotalSeconds) * 2;
                
                Position = new Vector2(Position.X + swayOffset, hoverY);
            }
            // ---------------------------------------------

            attackTimer += gameTime.ElapsedGameTime.TotalSeconds;

            if (attackTimer > 2.0)
            {
                if (HP < 100)
                    SpecialAttack(player);
                else
                {
                    // Regular attack logic here
                }
                attackTimer = 0;
            }
        }

        public void SpecialAttack(Player player)
        {
            // Implement special attack logic
        }

        public void ChangePhase()
        {
            phases--;
            speed *= 1.5f;
            System.Diagnostics.Debug.WriteLine($"{Name} is enraged! Target its {weakSpots}!");
        }

        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);

            if (phases == 3 && HP <= 3000)
                ChangePhase();
            else if (phases == 2 && HP <= 1000)
                ChangePhase();
        }
    }
}