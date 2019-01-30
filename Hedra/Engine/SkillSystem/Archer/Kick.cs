/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 17/03/2017
 * Time: 07:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Archer
{
    /// <summary>
    /// Description of Bash.
    /// </summary>
    public class Kick : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Kick.png");    
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/ArcherKick.dae");
        private const float BaseDamage = 15;
        private float Damage => BaseDamage + (BaseDamage * Level * .5f);
        public override float MaxCooldown => Math.Max(8, 12 - Level * .5f);
        public override float ManaCost => 40;
        protected override float AnimationSpeed => 1.5f;
        protected override int MaxLevel => 20;
        private bool _emitParticles;

        public override bool MeetsRequirements()
        {
            return base.MeetsRequirements() && !Player.IsAttacking && !Player.IsCasting;
        }

        protected override void OnAnimationMid()
        {
            SkillUtils.DamageNearby(Player, Damage, 16, .25f);
            _emitParticles = true;
            SoundPlayer.PlaySound(SoundType.SwooshSound, Player.Position, false, 0.8f, 1f);
        }

        protected override void OnAnimationEnd()
        {
            _emitParticles = false;
        }

        protected override void OnExecution()
        {
            Player.Movement.Orientate();
            PushEntitiesAway();
            
            if(_emitParticles) return;
            World.Particles.Color = new Vector4(1,1,1,1);
            World.Particles.ParticleLifetime = 1f;
            World.Particles.GravityEffect = .0f;
            World.Particles.Direction = Vector3.Zero;
            World.Particles.Scale = new Vector3(.5f,.5f,.5f);
            World.Particles.Position = 
                Player.Model.TransformFromJoint(Player.Model.JointDefaultPosition(Player.Model.RightFootJoint)
                                                + Vector3.UnitZ * 3f, Player.Model.RightFootJoint);
            World.Particles.PositionErrorMargin = Vector3.One * 0.75f;
                    
            for(var i = 0; i < 2; i++)
                World.Particles.Emit();
        }

        private void PushEntitiesAway()
        {
            for(var i = 0; i< World.Entities.Count; i++)
            {
                if (Player == World.Entities[i]) continue;
                if (!((Player.Position - World.Entities[i].Position).LengthSquared < 32 * 32)) continue;                  
                var direction = -(Player.Position - World.Entities[i].Position).NormalizedFast();
                World.Entities[i].Physics.DeltaTranslate(direction * 64f);
            }
        }
        
        public override string Description => Translations.Get("kick_desc");
        public override string DisplayName => Translations.Get("kick_skill");
    }
}
