/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:13 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.PhysicsSystem;
using OpenTK;
using System.Collections.Generic;
using Hedra.Engine.Generation;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of WeaponThrow.
	/// </summary>
	public class RoundSlash : Skill
	{
		private Animation RoundSlashAnimation;
		private float FrameCounter = 0, PassedTime = 0, Damage = 0;
		private Dictionary<Entity, float> AffectedEntities = new Dictionary<Entity, float>();
		
		public RoundSlash(Vector2 Position, Vector2 Scale, Panel InPanel, LocalPlayer Player) : base(Position, Scale, InPanel, Player) {
			base.TexId = Graphics2D.LoadFromAssets("Assets/Skills/RoundSlash.png");
			base.ManaCost = 80f;
			base.MaxCooldown = 8.5f;
			
			RoundSlashAnimation = AnimationLoader.LoadAnimation("Assets/Chr/RogueBladeRoundAttack.dae");
			RoundSlashAnimation.Loop = false;
			RoundSlashAnimation.Speed = 1.75f;
			RoundSlashAnimation.OnAnimationStart += delegate(Animation Sender) { 
				Sound.SoundManager.PlaySound(Sound.SoundType.SwooshSound, Player.Position, false, 0.8f, 1f);
			};
			RoundSlashAnimation.OnAnimationEnd += delegate(Animation Sender) {
				Player.IsCasting = false;
				Casting = false;
				Player.IsAttacking = false;
				Player.Model.LeftWeapon.InAttackStance = false;
			};
		}

		public override void KeyDown(){
			base.MaxCooldown = 9f - Math.Min(5f, base.Level * .5f);
			this.Damage = 1.0f + Math.Min(1f, base.Level * 0.15f);
			Player.IsCasting = true;
			Casting = true;
			Player.IsAttacking = true;
			Player.Model.Model.Animator.StopBlend();
			Player.Model.Model.PlayAnimation(RoundSlashAnimation);
			Player.Movement.OrientatePlayer(Player);
			Player.Model.LeftWeapon.InAttackStance = true;
		}
		
		public override void Update(){
			if(Player.IsCasting && Casting){
				
				World.WorldParticles.Color = new Vector4(1,1,1,1);
				World.WorldParticles.ParticleLifetime = 1f;
				World.WorldParticles.GravityEffect = .0f;
				World.WorldParticles.Direction = Vector3.Zero;
				World.WorldParticles.Scale = new Vector3(.15f,.15f,.15f);
				World.WorldParticles.Position = Player.Model.Model.TransformFromJoint(Player.Model.Model.JointDefaultPosition(Player.Model.LeftHand)
				                                                                             + Vector3.UnitZ *0f, Player.Model.LeftHand);
				World.WorldParticles.PositionErrorMargin = Vector3.One * 0.75f;
				
				for(int i = 0; i < 1; i++)
					World.WorldParticles.Emit();
				
				

				for(int i = World.Entities.Count-1; i > -1; i--){

					if( (World.Entities[i].Position  - Player.Position).LengthFast <
					   (this.Player.DefaultBox.Max - this.Player.DefaultBox.Min).LengthFast +
					   (World.Entities[i].DefaultBox.Max - World.Entities[i].DefaultBox.Min).LengthFast - 2
					   && World.Entities[i] != Player && !World.Entities[i].IsStatic){
						float Dmg = Player.DamageEquation * Damage * Engine.Time.ScaledFrameTimeSeconds * 2f;
						if(AffectedEntities.ContainsKey(World.Entities[i])){
				 			AffectedEntities[World.Entities[i]] = AffectedEntities[World.Entities[i]] + Dmg;
						}else{
							AffectedEntities.Add(World.Entities[i], Dmg);
						}
					}
				}
				
				if(FrameCounter >= .3f){
					foreach(Entity Key in AffectedEntities.Keys){
						float Exp;
						Key.Damage(AffectedEntities[Key], Player, out Exp, true);
						Player.XP += Exp;
					}
					AffectedEntities.Clear();
					FrameCounter = 0;
				}
				PassedTime += Engine.Time.ScaledFrameTimeSeconds;
				FrameCounter += Engine.Time.ScaledFrameTimeSeconds;
			}
		}
		
		public override string Description {
			get {
				return "Cast a special attack which damages surrounding enemies.";
			}
		}
	}
}