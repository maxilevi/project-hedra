/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 17/03/2017
 * Time: 07:23 p.m.
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
using Hedra.Engine.Player.Skills;
using OpenTK;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of Bash.
	/// </summary>
	public class Kick : BaseSkill
	{	
		private float Damage = 35f;
		private bool EmitParticles = false;
		private Animation KickAnimation;
		
		public Kick(Vector2 Position, Vector2 Scale, Panel InPanel, LocalPlayer Player) : base(Position, Scale, InPanel, Player) {
			base.TexId = Graphics2D.LoadFromAssets("Assets/Skills/Kick.png");
			base.ManaCost = 15f;
			base.MaxCooldown = 3f;
			
			this.KickAnimation = AnimationLoader.LoadAnimation("Assets/Chr/ArcherKick.dae");//ArcherKick
			this.KickAnimation.Loop = false;
			
			this.KickAnimation.OnAnimationEnd += delegate { 
				Player.IsCasting = false;
				Casting = false;
				Player.Movement.MoveFeet = false;
				Player.Movement.MoveCount = 1.0f;
				Player.WasAttacking = false;
				Player.IsAttacking = false;
				this.EmitParticles = false;
			};
			
			this.KickAnimation.OnAnimationMid += delegate {
			
				for(int i = 0; i< World.Entities.Count; i++){
					if(World.Entities[i] == Player)
						continue;
					
					Vector3 ToEntity = (World.Entities[i].Position - Player.Position).NormalizedFast();
					float Dot = Mathf.DotProduct(ToEntity, Player.Orientation);
					if(Dot >= .25f && (World.Entities[i].Position - Player.Position).LengthSquared < 16f*16f){
						float Exp;
						World.Entities[i].Damage(this.Damage * Dot * 1.25f, Player, out Exp, true);
						Player.XP += Exp;
					}
				}
				this.EmitParticles = true;
				Sound.SoundManager.PlaySound(Sound.SoundType.SwooshSound, Player.Position, false, 0.8f, 1f);
			};
		}
		
		public override bool MeetsRequirements(AbilityBarSystem.Toolbar Bar, int CastingAbilityCount)
		{
			return base.MeetsRequirements(Bar, CastingAbilityCount) && !Player.IsAttacking && !Player.IsCasting;
		}
		
		public override void KeyDown(){
			base.MaxCooldown = Math.Max(4f - base.Level * .25f, 1.5f);
			Damage = 35f * base.Level * .6f + 5f;
			Player.IsCasting = true;
			Casting = true;
			Player.IsAttacking = false;
			Player.WasAttacking = false;
			Player.Model.LeftWeapon.SlowDown = false;
			Player.Model.Model.Animator.StopBlend();
			Player.Model.Model.PlayAnimation(KickAnimation);
		}
		
		public override void Update(){
			if(Player.IsCasting && Casting){
				Player.Movement.OrientatePlayer(Player);
				
				if(EmitParticles){
					this.PushEntitiesAway();
					World.WorldParticles.Color = new Vector4(1,1,1,1);
					World.WorldParticles.ParticleLifetime = 1f;
					World.WorldParticles.GravityEffect = .0f;
					World.WorldParticles.Direction = Vector3.Zero;
					World.WorldParticles.Scale = new Vector3(.5f,.5f,.5f);
					World.WorldParticles.Position = Player.Model.Model.TransformFromJoint(Player.Model.Model.JointDefaultPosition(Player.Model.RightFoot)
					                                                                             + Vector3.UnitZ *3f, Player.Model.RightFoot);
					World.WorldParticles.PositionErrorMargin = Vector3.One * 0.75f;
					
					for(int i = 0; i < 2; i++)
						World.WorldParticles.Emit();
				}
			}
		}
		
		public void PushEntitiesAway(){
			for(int i = 0; i< World.Entities.Count; i++){
				if( (Player.Position - World.Entities[i].Position).LengthSquared < 48*48){
					if(Player == World.Entities[i])
						continue;
					
					Vector3 Direction = -(Player.Position - World.Entities[i].Position).Normalized();
					World.Entities[i].BlockPosition += Direction * (float) Engine.Time.deltaTime * 96f;
				}
			}
		}
		
		public override string Description => "A powerful knocking kick.";
	}
}
