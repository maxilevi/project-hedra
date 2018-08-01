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
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem.WeaponSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player.ToolbarSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Player.Skills
{
	/// <summary>
	/// Description of ArcherPoisonArrow.
	/// </summary>
	public class ArcherFlameArrow : SpecialAttackSkill<Bow>
	{
		private const float BaseDamage = 80f;
		private const float EffectDuration = 6;
		private const float EffectRange = 24;
		public override uint TextureId => Graphics2D.LoadFromAssets("Assets/Skills/FlameArrow.png");
		public override string Description => "Shoot a flaming arrow.";

		protected override void BeforeUse(Bow Weapon)
		{
			void HandlerLambda(Projectile A) => ModifierHandler(Weapon, A, HandlerLambda);
			Weapon.BowModifiers += HandlerLambda;
		}

		private void ModifierHandler(Bow Weapon, Projectile Arrow, OnModifyArrowEvent Event)
		{
			Arrow.MoveEventHandler += Sender =>
			{
				Arrow.Mesh.Tint = Bar.Low * new Vector4(1, 3, 1, 1) * .7f;

				World.Particles.Color = Particle3D.FireColor;
				World.Particles.VariateUniformly = false;
				World.Particles.Position = Sender.Mesh.Position;
				World.Particles.Scale = Vector3.One * .5f;
				World.Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
				World.Particles.Direction = Vector3.UnitY * .2f;
				World.Particles.ParticleLifetime = 0.75f;
				World.Particles.GravityEffect = 0.0f;
				World.Particles.PositionErrorMargin = new Vector3(1.5f, 1.5f, 1.5f);

				for (var i = 0; i < 2; i++)
					World.Particles.Emit();
			};
			Arrow.LandEventHandler += delegate 
			{ 
				CoroutineManager.StartCoroutine(CreateFlames, Arrow);
			};
			Arrow.HitEventHandler += delegate(Projectile Sender, Entity Hit)
			{				
				Hit.AddComponent( new BurningComponent(Hit, Player, 3 + Utils.Rng.NextFloat() * 2f, BaseDamage) );
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

				for (var i = 0; i < 4; i++)
				{
					World.Particles.Emit();
				}
				World.Entities.ToList().ForEach(delegate(Entity Entity)
				{
					if (!((Entity.Position - position).LengthSquared < EffectRange * EffectRange) || Entity.IsStatic) return;
					
					if(Entity.SearchComponent<BurningComponent>() == null)
					{
						Entity.AddComponent(new BurningComponent(Entity, Player, EffectDuration, BaseDamage * .4f));
					}
				});
				yield return null;
			}
		}
	}
}
