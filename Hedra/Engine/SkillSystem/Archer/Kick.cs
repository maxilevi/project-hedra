/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 17/03/2017
 * Time: 07:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Globalization;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Archer
{
    /// <summary>
    /// Description of Bash.
    /// </summary>
    public class Kick : SingleAnimationSkill<IPlayer>
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Kick.png");    
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/ArcherKick.dae");
        private const float BaseDamage = 15;
        private float Damage => BaseDamage + (BaseDamage * Level * .5f);
        public override float MaxCooldown => Math.Max(8, 12 - Level * .5f);
        public override float ManaCost => 40;
        protected override float AnimationSpeed => 1.3f;
        protected override int MaxLevel => 20;
        protected override bool ShouldDisable => User.IsAttacking || User.IsCasting;
        private bool _emitParticles;
        private bool _shouldPush;

        protected override void OnAnimationMid()
        {
            _shouldPush = true;
            SkillUtils.DoNearby(User, 24, .25f, (E, D) =>
            {
                if(Utils.Rng.Next(0, 4) == 1 && !E.IsKnocked) E.KnockForSeconds(3f);
            });
            SoundPlayer.PlaySound(SoundType.SwooshSound, User.Position, false, 0.8f, 1f);
        }

        protected override void OnAnimationStart()
        {
            _emitParticles = true;
        }

        protected override void OnAnimationEnd()
        {
            _emitParticles = false;
            _shouldPush = false;
        }

        protected override void OnExecution()
        {
            User.Movement.Orientate();
            if(_shouldPush)
                PushEntitiesAway();
            SkillUtils.DamageNearby(User, Damage * Time.DeltaTime, 24, .25f, false);
            
            if(_emitParticles) return;
            World.Particles.Color = new Vector4(1,1,1,1);
            World.Particles.ParticleLifetime = 1f;
            World.Particles.GravityEffect = .0f;
            World.Particles.Direction = Vector3.Zero;
            World.Particles.Scale = new Vector3(.5f,.5f,.5f);
            World.Particles.Position = 
                User.Model.TransformFromJoint(User.Model.JointDefaultPosition(User.Model.RightFootJoint)
                                                + Vector3.UnitZ * 3f, User.Model.RightFootJoint);
            World.Particles.PositionErrorMargin = Vector3.One * 0.75f;
                    
            for(var i = 0; i < 2; i++)
                World.Particles.Emit();
        }

        private void PushEntitiesAway()
        {
            for(var i = 0; i< World.Entities.Count; i++)
            {
                if (User == World.Entities[i]) continue;
                if (!((User.Position - World.Entities[i].Position).LengthSquared < 24 * 24)) continue;                  
                var direction = -(User.Position - World.Entities[i].Position).NormalizedFast();
                World.Entities[i].Physics.DeltaTranslate(direction * 64f);
            }
        }
        
        public override string Description => Translations.Get("kick_desc");
        public override string DisplayName => Translations.Get("kick_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("kick_damage_change", Damage.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}
