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
using System.Linq;
using Hedra.Components.Effects;
using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Particles;
using Hedra.EntitySystem;
using Hedra.WeaponSystem;
using OpenTK;

namespace Hedra.Engine.Player.Skills.Archer
{
    /// <summary>
    /// Description of ArcherPoisonArrow.
    /// </summary>
    public class FlameArrow : SpecialAttackSkill<Bow>
    {
        private const float BaseDamage = 80f;
        private const float BaseCooldown = 24f;
        private const float CooldownCap = 12f;
        private const float RangeCap = 12f;
        private const float DurationCap = 12f;
        private const float BaseEffectDuration = 6;
        private const float BaseEffectRange = 24;
        private const float BaseManaCost = 40f;
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/FlameArrow.png");
        public override string Description => "Shoot a flaming arrow.";
        public override string DisplayName => "Flame Arrow";
        private float Damage => BaseDamage * (base.Level * 0.40f) + BaseDamage;
        public override float MaxCooldown => Math.Max(BaseCooldown - 0.80f * base.Level, CooldownCap);
        private float EffectDuration => Math.Min(BaseEffectDuration + 0.15f * base.Level, DurationCap);
        private float EffectRange => Math.Min(BaseEffectRange + 0.15f * base.Level, RangeCap);
        public override float ManaCost => BaseManaCost;

        protected override void BeforeUse(Bow Weapon)
        {
            void HandlerLambda(Projectile A) => ModifierHandler(Weapon, A, HandlerLambda);
            Weapon.BowModifiers += HandlerLambda;
        }

        private void ModifierHandler(Bow Weapon, Projectile Arrow, OnModifyArrowEvent Event)
        {
            Arrow.MoveEventHandler += Sender =>
            {
                Arrow.Mesh.Tint = Colors.LowHealthRed * new Vector4(1, 3, 1, 1) * .7f;

                World.Particles.Color = Particle3D.FireColor;
                World.Particles.VariateUniformly = false;
                World.Particles.Position = Sender.Mesh.Position;
                World.Particles.Scale = Vector3.One * .5f;
                World.Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
                World.Particles.Direction = Vector3.UnitY * .2f;
                World.Particles.ParticleLifetime = 0.75f;
                World.Particles.GravityEffect = 0.0f;
                World.Particles.PositionErrorMargin = new Vector3(1.5f, 1.5f, 1.5f);
                World.Particles.Emit();
            };
            Arrow.LandEventHandler += delegate 
            { 
                CoroutineManager.StartCoroutine(CreateFlames, Arrow);
            };
            Arrow.HitEventHandler += delegate(Projectile Sender, IEntity Hit)
            {                
                Hit.AddComponent( new BurningComponent(Hit, Player, 3 + Utils.Rng.NextFloat() * 2f, Damage) );
                CoroutineManager.StartCoroutine( this.CreateFlames, Arrow);
            };
            Weapon.BowModifiers -= Event;
        }

        private IEnumerator CreateFlames(object[] Params)
        {
            var arrowProj = (Projectile) Params[0];
            var position = arrowProj.Mesh.Position;    
            var time = 0f;
            World.HighlightArea(position, Particle3D.FireColor, EffectRange, EffectDuration);
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
                        Entity.AddComponent(new BurningComponent(Entity, Player, EffectDuration, Damage * .25f));
                    }
                });
                yield return null;
            }
        }
    }
}
