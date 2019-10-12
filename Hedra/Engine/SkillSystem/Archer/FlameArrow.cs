/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 30/04/2017
 * Time: 10:22 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using Hedra.Components.Effects;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Particles;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.WeaponSystem;
using Hedra.WorldObjects;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.SkillSystem.Archer
{
    /// <summary>
    /// Description of ArcherPoisonArrow.
    /// </summary>
    public class FlameArrow : SpecialRangedAttackSkill
    {
        private const float BaseDamage = 60f;
        private const float BaseCooldown = 24f;
        private const float CooldownCap = 12f;
        private const float RangeCap = 12f;
        private const float DurationCap = 12f;
        private const float BaseEffectDuration = 6;
        private const float BaseEffectRange = 24;
        private const float BaseManaCost = 40f;
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/FlameArrow.png");
        public override string Description => Translations.Get("flame_arrow_desc");
        public override string DisplayName => Translations.Get("flame_arrow");
        private float Damage => BaseDamage * (base.Level * 0.75f) + BaseDamage;
        public override float MaxCooldown => Math.Max(BaseCooldown - 0.80f * base.Level, CooldownCap);
        private float EffectDuration => Math.Min(BaseEffectDuration + 0.15f * base.Level, DurationCap);
        private float EffectRange => Math.Min(BaseEffectRange + 0.15f * base.Level, RangeCap);
        public override float ManaCost => BaseManaCost;
        protected override int MaxLevel => 99;

        protected override void OnHit(Projectile Proj, IEntity Victim)
        {
            base.OnHit(Proj, Victim);
            Victim.AddComponent( new BurningComponent(Victim, User, 3 + Utils.Rng.NextFloat() * 2f, Damage) );
            RoutineManager.StartRoutine( this.CreateFlames, Proj);
        }

        protected override void OnLand(Projectile Proj, LandType Type)
        {
            base.OnLand(Proj, Type);
            RoutineManager.StartRoutine(CreateFlames, Proj);
        }

        protected override void OnMove(Projectile Proj)
        {
            base.OnMove(Proj);
            Proj.Mesh.Tint = Colors.LowHealthRed * new Vector4(1, 3, 1, 1) * .7f;
            World.Particles.Color = Particle3D.FireColor;
            World.Particles.VariateUniformly = false;
            World.Particles.Position = Proj.Mesh.Position;
            World.Particles.Scale = Vector3.One * .5f;
            World.Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
            World.Particles.Direction = Vector3.UnitY * .2f;
            World.Particles.ParticleLifetime = 0.75f;
            World.Particles.GravityEffect = 0.0f;
            World.Particles.PositionErrorMargin = new Vector3(1.5f, 1.5f, 1.5f);
            World.Particles.Emit();
        }

        private IEnumerator CreateFlames(object[] Params)
        {
            var arrowProj = (Projectile) Params[0];
            var position = arrowProj.Mesh.Position;    
            var time = 0f;
            World.HighlightArea(position, Particle3D.FireColor, EffectRange * 1.5f, EffectDuration);
            while(time < EffectDuration)
            {
                time += Time.DeltaTime;            
                World.Particles.Color = Particle3D.FireColor;
                World.Particles.VariateUniformly = false;
                World.Particles.Position = position;
                World.Particles.Scale = Vector3.One * .5f;
                World.Particles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);
                World.Particles.Direction = Vector3.UnitY * 1.5f;
                World.Particles.ParticleLifetime = 0.75f;
                World.Particles.GravityEffect = 0.5f;
                World.Particles.PositionErrorMargin = new Vector3(12f, 4f, 12f);
                World.Particles.Emit();
                
                World.Entities.ToList().ForEach(delegate(IEntity Entity)
                {
                    if (!((Entity.Position - position).LengthSquared < EffectRange * EffectRange) || Entity.IsStatic) return;
                    
                    if(Entity.SearchComponent<BurningComponent>() == null)
                    {
                        Entity.AddComponent(new BurningComponent(Entity, User, EffectDuration, Damage * .25f));
                    }
                });
                yield return null;
            }
        }
        public override string[] Attributes => new[]
        {
            Translations.Get("flame_arrow_damage_change", Damage.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("flame_arrow_duration_change", EffectDuration.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("flame_arrow_radius_change", EffectRange.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}
