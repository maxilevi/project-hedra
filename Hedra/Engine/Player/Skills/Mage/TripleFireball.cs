/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 09/08/2016
 * Time: 08:16 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Rendering.Particles;
using OpenTK;

namespace Hedra.Engine.Player.Skills.Mage
{
    /// <summary>
    /// Description of TripleFireball.
    /// </summary>
    public class TripleFireball : BaseSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/TripleFireball.png");
        private bool LeftHand;
        private float Damage = 6.5f;
        private int FireballCount = 0;
        private ParticleSystem Particles = new ParticleSystem(Vector3.Zero);
        private Animation FireballAnimation;
        public override string DisplayName => "Triple Fireball";
        
        public TripleFireball() : base() {
            base.MaxCooldown = 2.5f;
            base.ManaCost = 35f;
            
            /*FireballAnimation = AnimationLoader.LoadAnimation("Assets/Chr/MageFireball.dae");
            FireballAnimation.OnAnimationEnd += delegate(Animation Sender) {
                Player.IsCasting = false;
                Casting = false;
                Sound.SoundManager.PlaySound(Sound.SoundType.SWOOSH_SOUND, Player.Position, false, 0.8f, 1f);
                CoroutineManager.StartCoroutine(ShootCoroutine);
                
            };*/
        }
        
        private int FireballCombo = 5;
        public override void Use(){
            base.MaxCooldown = 2.75f - base.Level * .15f;
            Player.IsCasting = true;
            Casting = true;
            LeftHand = !LeftHand;
            FireballCount++;
            if(FireballCount > FireballCombo)
                FireballCount = 0;
            Player.Model.PlayAnimation(FireballAnimation);
        }
        
        public override void Update(){ }

        public void CreateProjectile(Vector3 Direction)
        {
            /*
            float RandomScale = Mathf.Clamp(Utils.Rng.NextFloat() * 2f -1f, 1, 2);
            ParticleProjectile Fire = new ParticleProjectile(Vector3.One + new Vector3(RandomScale, RandomScale, RandomScale) * 0.35f,
                                        ((LeftHand) ? Player.Model.LeftWeaponPosition - Vector3.UnitX * .5f : Player.Model.RightWeaponPosition + Vector3.UnitX * .5f) + Player.Orientation * 2 + Vector3.UnitY * 2f,
                                        2, Direction, Player);
            
            Fire.Trail = true;
            Fire.UseLight = true;
            
            Particles.Position = Fire.Position;
            Particles.Color = Particle3D.FireColor;
            Particles.Direction = Fire.Direction;
            Particles.ParticleLifetime = 5;
            Particles.GravityEffect = 0f;
            Particles.PositionErrorMargin = new Vector3(1f,1f,1f);
            Particles.Scale = Vector3.One * .5f;
            Particles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);

            for(int i = 0; i < 30; i++){
                Particles.Emit();
            }
            
            Fire.HitEventHandler += delegate(ParticleProjectile Sender, Entity Hit) { 
                float Exp;
                Hit.Damage(Damage * Math.Max(1, base.Level * .5f), Player, out Exp);
                Player.XP += Exp;
                Particles.Position = Hit.Position; 
                Particles.PositionErrorMargin = new Vector3(1f,1f,1f);
                Particles.ParticleLifetime = 0.5f;
                for(int i = 0; i < 50; i++){
                    Vector3 Dir = (Mathf.RandomVector3(Utils.Rng) - Vector3.One * 0.5f);
                    Particles.Direction = Dir;
                    Particles.Emit();
                }
                Sender.Dispose();
            };*/
        }
        
        public override string Description => "Shoot 5 fireballs in different directions.";
    }
}
