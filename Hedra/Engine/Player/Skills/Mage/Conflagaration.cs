/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 04/07/2016
 * Time: 05:18 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Rendering.Particles;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.Player.Skills.Mage
{
    /// <summary>
    /// Description of Conflagaration.
    /// </summary>
    public class Conflagaration : BaseSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Conflagaration.png");
        private float Damage = 22.5f;
        private PointLight Light;
        private Timer LightTimer = new Timer(.35f);
        private Animation ConflagarationAnimation;
        
        public Conflagaration() : base() {
            base.MaxCooldown = 8f;
            base.ManaCost = 35f;
            
            /*ConflagarationAnimation = AnimationLoader.LoadAnimation("Assets/Chr/MageConflagaration.dae");
            
            ConflagarationAnimation.OnAnimationEnd += delegate(Animation Sender) { 
                Sound.SoundManager.PlaySound(Sound.SoundType.SWOOSH_SOUND, Player.Position, false, 0.8f, 1f);
                this.CreateEffect(Player.Position);
                this.DamageEntities();
            };
                
            ConflagarationAnimation.OnAnimationEnd += delegate(Animation Sender) {
                Player.Movement.Check = true;
                Player.IsCasting = false;
                Casting = false;
            };*/
        }
        
        public override void Use(){
            Player.IsCasting = true;
            Casting = true;
            base.MaxCooldown = 8 - 1f * base.Level;
            Player.Movement.CaptureMovement = false;
            
            Light = ShaderManager.GetAvailableLight();
            Light.Color = new Vector3(1,0.2f,0.2f);
            LightTimer.Reset();
            Player.Model.PlayAnimation(ConflagarationAnimation);
        }
    
        public override void Update(){
            if(LightTimer.Tick() && Light != null){
                Light.Locked = false;
                Light.Position = Vector3.Zero;
                ShaderManager.UpdateLight(Light);
                Light = null;
            }
            if(Casting && Player.IsCasting){
                this.PushEntitiesAway();
                Player.Movement.Orientate();
            }
            
        }
        
        private void CreateEffect(Vector3 Position){
            float RandomScale = Mathf.Clamp(Utils.Rng.NextFloat() * 2f -1f, 1, 2);
            
            ParticleSystem Particles = new ParticleSystem(Position);
            Particles.Color = Particle3D.FireColor;
            Particles.GravityEffect = 0f;
            Particles.Scale = Vector3.One * .5f;
            Particles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);
            Particles.PositionErrorMargin = new Vector3(2.00f,2.00f,2.00f);
            Particles.Shape = ParticleShape.Sphere;
            
            Particles.ParticleLifetime = .25f;
            for(int i = 0; i < 750; i++){
                Vector3 Dir = (Mathf.RandomVector3(Utils.Rng) * 2f - Vector3.One);
                Particles.Direction = Dir * .75f;
                Particles.Emit();
            }
        }
        
        public void PushEntitiesAway(){
            for(int i = 0; i< World.Entities.Count; i++){
                if( (Player.Position - World.Entities[i].Position).LengthSquared < 16*16){
                    if(Player == World.Entities[i])
                        continue;
                    
                    Vector3 Direction = -(Player.Position - World.Entities[i].Position).Normalized();
                    World.Entities[i].BlockPosition += Direction * (float) Engine.Time.DeltaTime * 36f;
                }
            }
        }
        
        public override string Description => "Create a fire explosion that damages and throws enemies out of your range.";

        public override string DisplayName => "Conflagaration";
        
        public void DamageEntities(){
            bool Hitted = false;
            for(int i = 0; i< World.Entities.Count; i++){
                if( (Player.Position - World.Entities[i].Position).LengthSquared < 16*16){
                    if(Player == World.Entities[i])
                        continue;
                    
                    float Exp;
                    World.Entities[i].Damage(Damage * base.Level, Player, out Exp, false);
                    Player.XP += Exp;
                    Hitted = true;
                }
            }
            if(Hitted)
                SoundPlayer.PlaySound(SoundType.HitSound, Player.Position, false, 1f,.6f);
        }
        
        
    }
}
