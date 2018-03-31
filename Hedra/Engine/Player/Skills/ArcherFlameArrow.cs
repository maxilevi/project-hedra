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
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem.WeaponSystem;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Rendering.Animation;
using OpenTK;
using Hedra.Engine.Rendering.Particles;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of ArcherPoisonArrow.
	/// </summary>
	public class FlameArrow : BaseSkill
	{
		private Animation ShootAnimation;
		private float BaseDamage = 50f, Damage;
		
		public FlameArrow(Vector2 Position, Vector2 Scale, Panel InPanel, LocalPlayer Player) : base(Position, Scale, InPanel, Player) {
			base.TexId = Graphics2D.LoadFromAssets("Assets/Skills/FlameArrow.png");
			base.ManaCost = 85f;
			base.MaxCooldown = 6.5f;

            ShootAnimation = AnimationLoader.LoadAnimation("Assets/Chr/ArcherTripleShoot.dae");
			ShootAnimation.Loop = false;
			ShootAnimation.OnAnimationMid += delegate {

				if(Player.Model.LeftWeapon is Bow){
					Bow PlayerBow = Player.Model.LeftWeapon as Bow;
					
					Projectile Arrow = PlayerBow.ShootArrow(Player, Player.View.CrossDirection);
					Arrow.MoveEventHandler += delegate(Projectile Sender) { 
						Arrow.Mesh.Tint = Bar.Low * new Vector4(1,3,1,1) * .7f;
						
						World.WorldParticles.Color = Particle3D.FireColor;
						World.WorldParticles.VariateUniformly = false;
						World.WorldParticles.Position = Sender.Mesh.Position;
						World.WorldParticles.Scale = Vector3.One * .5f;
						World.WorldParticles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);
						World.WorldParticles.Direction = Vector3.UnitY * .2f;
						World.WorldParticles.ParticleLifetime = 0.75f;
						World.WorldParticles.GravityEffect = 0.0f;
						World.WorldParticles.PositionErrorMargin = new Vector3(1.5f, 1.5f, 1.5f);
						
						for(int i = 0; i < 2; i++)
							World.WorldParticles.Emit();
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
		
		public IEnumerator CreateFlames(object[] Params){
			Projectile ArrowProj = Params[0] as Projectile;
			Vector3 Position = ArrowProj.Mesh.Position;
			
			ParticleSystem Particles = new ParticleSystem(Position);
			Particles.Color = Particle3D.FireColor;
			Particles.GravityEffect = 0f;
			Particles.Scale = Vector3.One * .5f;
			Particles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);
			Particles.PositionErrorMargin = new Vector3(2.00f,2.00f,2.00f);
			Particles.Shape = ParticleShape.SPHERE;
			
			Particles.ParticleLifetime = .75f;
			for(int i = 0; i < 750; i++){
				Vector3 Dir = (Mathf.RandomVector3(Utils.Rng) * 2f - Vector3.One);
				Particles.Direction = Dir * 1f;
				Particles.Emit();
			}
			
			World.HighlightArea(Position, Particle3D.FireColor, 24f, 6f);
			
			float Time = 0;
			while(Time < 6){
				Time += Engine.Time.ScaledFrameTimeSeconds;
			
				World.WorldParticles.Color = Particle3D.FireColor;
				World.WorldParticles.VariateUniformly = false;
				World.WorldParticles.Position = Position;
				World.WorldParticles.Scale = Vector3.One * .5f;
				World.WorldParticles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);
				World.WorldParticles.Direction = Vector3.UnitY * 1.5f;
				World.WorldParticles.ParticleLifetime = 0.75f;
				World.WorldParticles.GravityEffect = 0.5f;
				World.WorldParticles.PositionErrorMargin = new Vector3(12f, 4f, 12f);
				
				for(int i = 0; i < 4; i++)
					World.WorldParticles.Emit();
				
				for(int i = World.Entities.Count-1; i > -1; i--){
					if( (World.Entities[i].Position - Position).LengthSquared < 24f*24f && !World.Entities[i].IsStatic){
						if(World.Entities[i].SearchComponent<BurningComponent>() == null){
							World.Entities[i].AddComponent( new BurningComponent(World.Entities[i], Player, 6f, Damage * .4f) );
						}
					}
				}
				
				yield return null;
			}
		}
		
		public override bool MeetsRequirements(AbilityBarSystem.Toolbar Bar, int CastingAbilityCount)
		{
			return base.MeetsRequirements(Bar, CastingAbilityCount) && Player.Model.LeftWeapon is Bow;
		}
		
		public override void KeyDown()
		{
			this.Damage = BaseDamage + 15f * base.Level;
			base.MaxCooldown = Math.Max(10f - base.Level * .5f, 4f);
			Player.IsCasting = true;
			Casting = true;
			Player.IsAttacking = true;
			Player.Model.LeftWeapon.InAttackStance = true;
			Player.Model.Model.Animator.StopBlend();
			Player.Model.Model.PlayAnimation(ShootAnimation);
			Player.Movement.OrientatePlayer( Player );
		}
		
		public override void Update(){}
		
		
		public override string Description {
			get {
				return "Shoot a poisonous arrow.";
			}
		}
	}
}
