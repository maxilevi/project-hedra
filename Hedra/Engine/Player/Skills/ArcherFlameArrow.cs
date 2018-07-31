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
	internal class ArcherFlameArrow : SpecialAttackSkill
	{
		private const float EffectDuration = 6;
		private const float EffectRange = 24;
		public override uint TexId => Graphics2D.LoadFromAssets("Assets/Skills/FlameArrow.png");
		public override string Description => "Shoot a flaming arrow.";
		
		public ArcherFlameArrow() : base() {
            ShootAnimation = AnimationLoader.LoadAnimation("Assets/Chr/ArcherTripleShoot.dae");
			ShootAnimation.Loop = false;
			ShootAnimation.OnAnimationMid += delegate {

				if(Player.Model.LeftWeapon is Bow){
					Bow PlayerBow = Player.Model.LeftWeapon as Bow;
					
					Projectile Arrow = PlayerBow.ShootArrow(Player, Player.View.CrossDirection);
					Arrow.MoveEventHandler += delegate(Projectile Sender) { 
						Arrow.Mesh.Tint = Bar.Low * new Vector4(1,3,1,1) * .7f;
						
						World.Particles.Color = Particle3D.FireColor;
						World.Particles.VariateUniformly = false;
						World.Particles.Position = Sender.Mesh.Position;
						World.Particles.Scale = Vector3.One * .5f;
						World.Particles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);
						World.Particles.Direction = Vector3.UnitY * .2f;
						World.Particles.ParticleLifetime = 0.75f;
						World.Particles.GravityEffect = 0.0f;
						World.Particles.PositionErrorMargin = new Vector3(1.5f, 1.5f, 1.5f);
						
						for(int i = 0; i < 2; i++)
							World.Particles.Emit();
					};
					Arrow.LandEventHandler += delegate(Projectile Sender) { 
						CoroutineManager.StartCoroutine( this.CreateFlames, new object[]{ Arrow } );
					};
					Arrow.HitEventHandler += delegate(Projectile Sender, Entity Hit) {
						float Exp;
						Hit.Damage(Player.DamageEquation * 0.5f, Player, out Exp, true);
						Player.XP += Exp;
						
						Hit.AddComponent( new BurningComponent(Hit, Player, 3 + Utils.Rng.NextFloat() * 2f, Damage) );
						CoroutineManager.StartCoroutine( this.CreateFlames, new object[]{ Arrow } );
					};
				}
				
			};
			ShootAnimation.OnAnimationEnd += delegate(Animation Sender) {
				
				Player.IsCasting = false;
				Casting = false;
				Player.IsAttacking = false;
				Player.Model.LeftWeapon.InAttackStance = false;
				Player.Model.LeftWeapon.StartWasAttackingCoroutine();
			};
		}
		
		
		public override void Use()
		{

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
				World.Entities.ToList().ForEach(delegate(Entity entity)
				{
					if (!((entity.Position - position).LengthSquared < EffectRange * EffectRange) || entity.IsStatic) return;
					
					if(entity.SearchComponent<BurningComponent>() == null)
					{
						entity.AddComponent(new BurningComponent(entity, Player, EffectDuration, Damage * .4f));
					}
				});
				yield return null;
			}
		}
	}
}
