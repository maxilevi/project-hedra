/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 08/07/2016
 * Time: 03:30 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Particles;
using OpenTK;

namespace Hedra.Engine.Player.Skills.Mage
{
    /// <summary>
    /// Description of FlameStyle.
    /// </summary>
    public class FlameStyle : BaseSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/FlameStyle.png");
        private const float Duration = 8f;
        private bool Emitting = false;
        private ParticleSystem HandParticles;
        public override string DisplayName => "Flame Style";
        
        public FlameStyle() : base() {
            base.MaxCooldown = 16.5f;
            base.ManaCost = 30f;
            HandParticles = new ParticleSystem(Vector3.Zero);
            HandParticles.Scale = new Vector3(.5f,.5f,.5f);
            HandParticles.Color = Particle3D.FireColor;
            HandParticles.ParticleLifetime = .5f;
        }
        
        public override void Use(){
            Emitting = true;
            base.MaxCooldown = 16.5f - 1.5f*base.Level;
            base.ManaCost = 30f - 5f * base.Level;
            Color = new Vector4(.25f,0,0,1);
        }
        
        public override string Description {
            get {
                return "Increase your strength temporarily.";
            }
        }
        
        private Vector4 Color;
        public override void Update(){
            if(Emitting){
                
                Player.Model.Tint = Color;
                
                if(base.Cooldown <= base.MaxCooldown - Duration){
                    Player.Model.Tint = new Vector4(0f,0f,0,1);
                    Emitting = false;
                }
            }
        }
    }
}
