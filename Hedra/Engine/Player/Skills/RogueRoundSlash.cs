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
using Hedra.Engine.Player.Skills;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of WeaponThrow.
	/// </summary>
	public class RoundSlash : BaseSkill
	{
		private Animation RoundSlashAnimation;
		private float FrameCounter = 0, PassedTime = 0, Damage = 0;
		private Dictionary<IEntity, float> AffectedEntities = new Dictionary<IEntity, float>();
		
		public RoundSlash() : base() {
			base.TextureId = Graphics2D.LoadFromAssets("Assets/Skills/RoundSlash.png");
			base.ManaCost = 80f;
			base.MaxCooldown = 8.5f;
			
			RoundSlashAnimation = AnimationLoader.LoadAnimation("Assets/Chr/RogueBladeRoundAttack.dae");
			RoundSlashAnimation.Loop = false;
			RoundSlashAnimation.Speed = 1.75f;
			RoundSlashAnimation.OnAnimationStart += delegate { 
				Sound.SoundManager.PlaySound(Sound.SoundType.SwooshSound, Player.Position, false, 0.8f, 1f);
			};
			RoundSlashAnimation.OnAnimationEnd += delegate {
				Player.IsCasting = false;
				Casting = false;
				Player.IsAttacking = false;
				Player.Model.LeftWeapon.InAttackStance = false;
			};
		}

		public override void Use(){
			base.MaxCooldown = 9f - Math.Min(5f, base.Level * .5f);
			this.Damage = 1.0f + Math.Min(1f, base.Level * 0.15f);
			Player.IsCasting = true;
			Casting = true;
			Player.IsAttacking = true;
			Player.Model.Model.Animator.StopBlend();
			Player.Model.Model.PlayAnimation(RoundSlashAnimation);
			Player.Movement.Orientate();
			Player.Model.LeftWeapon.InAttackStance = true;
		}
		
		public override void Update(){
			if(Player.IsCasting && Casting){
				
				World.Particles.Color = new Vector4(1,1,1,1);
				World.Particles.ParticleLifetime = 1f;
				World.Particles.GravityEffect = .0f;
				World.Particles.Direction = Vector3.Zero;
				World.Particles.Scale = new Vector3(.15f,.15f,.15f);
				World.Particles.Position = Player.Model.Model.TransformFromJoint(Player.Model.Model.JointDefaultPosition(Player.Model.LeftWeaponJoint)
				                                                                             + Vector3.UnitZ *0f, Player.Model.LeftWeaponJoint);
				World.Particles.PositionErrorMargin = Vector3.One * 0.75f;
				
				for(int i = 0; i < 1; i++)
					World.Particles.Emit();
				
				

				for(int i = World.Entities.Count-1; i > -1; i--){

					if(Player.InAttackRange(World.Entities[i]) && Player != World.Entities[i])
                    {
						float dmg = Player.DamageEquation * Damage * Engine.Time.DeltaTime * 4f;
						if(AffectedEntities.ContainsKey(World.Entities[i]))
                        {
				 			AffectedEntities[World.Entities[i]] = AffectedEntities[World.Entities[i]] + dmg;
						}else
                        {
							AffectedEntities.Add(World.Entities[i], dmg);
						}
					}
				}
				
				if(FrameCounter >= .3f){
					foreach(Entity Key in AffectedEntities.Keys)
					{
						float Exp;
						Key.Damage(AffectedEntities[Key], Player, out Exp, true);
						Player.XP += Exp;
					}
					AffectedEntities.Clear();
					FrameCounter = 0;
				}
				PassedTime += Time.DeltaTime;
				FrameCounter += Time.DeltaTime;
			}
		}
		
		public override string Description => "Cast a special attack which damages surrounding enemies.";
	}
}