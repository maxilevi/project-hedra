/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 30/07/2016
 * Time: 01:14 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using Hedra.Engine.Player;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering.Particles;

namespace Hedra.Engine.QuestSystem
{
	/// <summary>
	/// Description of TurtleBossAIComponent.
	/// </summary>
	public class FireGolemAIComponent : BossAIComponent
	{
		private ParticleSystem Particles = new ParticleSystem(Vector3.Zero);
		public Action AttackMode;
		public FireGolemAIComponent(Entity Parent) : base (Parent){
			AttackMode = () => NoAbility();
			base.AILogic = delegate{};
			
			Particles.ParticleLifetime = 5f;
			Particles.GravityEffect = 0f;
			Particles.PositionErrorMargin = Vector3.One;
			Particles.Color = Particle3D.FireColor;
			Particles.Scale = Vector3.One * .5f;
			Particles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);
		}
		
		public override void LateUpdate(){
			Player = GameManager.Player;
			if(Player == null) return;
			

			if(FireballCooldown < 0 && (Player.Position - Parent.Position).LengthSquared < 32*32 ){
				Particles.Position = Parent.Position;
				this.Fireball();
			}

			
			AttackMode();
			FireballCooldown -= Time.FrameTimeSeconds;
			
		}
		
		private float FireballCooldown = 5f;
		private void Fireball(){
			this.Parent.Model.Attack(Player, 0f);
			Physics.LookAt(Parent, Player);
			
			float RandomScale = Mathf.Clamp(Utils.Rng.NextFloat() * 2f -1f, 1, 2);
			var Fire = new ParticleProjectile(Vector3.One + new Vector3(RandomScale, RandomScale, RandomScale) * 0.35f,
			                            Parent.Position + Vector3.UnitZ * 0.5f + Vector3.UnitY,
			                            2, -(this.Parent.Position - Player.Position).NormalizedFast() * 4,  Parent);
			
			Fire.Trail = true;
			
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
				Hit.Damage(10 * Math.Max(1, Parent.Level * .5f), Parent, out Exp);
				
				Particles.Position = Hit.Position; 
				Particles.ParticleLifetime = 0.5f;
				for(int i = 0; i < 50; i++){
					Vector3 Dir = (Mathf.RandomVector3(Utils.Rng) - Vector3.One * 0.5f);
					Particles.Direction = Dir;
					Particles.Emit();
				}
				Sender.Dispose();
			};
			
			FireballCooldown = 5f;
		}
	}
}
