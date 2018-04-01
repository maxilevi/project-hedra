﻿/*
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
using Hedra.Engine.Generation;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Player.ToolbarSystem;
using Hedra.Engine.Rendering.Animation;
using OpenTK;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of WeaponThrow.
	/// </summary>
	public class Shuriken : BaseSkill
	{
		private Animation ThrowAnimation;
		private VertexData ShurikenData;
		
		public Shuriken(Vector2 Position, Vector2 Scale, Panel InPanel, LocalPlayer Player) : base(Position, Scale, InPanel, Player) {
			base.TexId = Graphics2D.LoadFromAssets("Assets/Skills/Shuriken.png");
			base.ManaCost = 35f;
			base.MaxCooldown = 8.5f;
            ShurikenData = AssetManager.PlyLoader("Assets/Items/Shuriken.ply", new Vector3(1,2,1) );
			
			ThrowAnimation = AnimationLoader.LoadAnimation("Assets/Chr/RogueShurikenThrow.dae");
			ThrowAnimation.Loop = false;
			ThrowAnimation.OnAnimationMid += delegate(Animation Sender) { 
				this.ShootShuriken(Player, Player.View.CrossDirection.NormalizedFast(), 8);
			};
			ThrowAnimation.OnAnimationEnd += delegate(Animation Sender) {
				Player.IsCasting = false;
				Casting = false;
				Player.IsAttacking = false;
			};
		}
		
		public override bool MeetsRequirements(Toolbar Bar, int CastingAbilityCount)
		{
			return base.MeetsRequirements(Bar, CastingAbilityCount);
		}
		
		private void ShootShuriken(Humanoid Human, Vector3 Direction, int KnockChance = -1){
			VertexData WeaponData = ShurikenData.Clone();
			WeaponData.Scale(Vector3.One * 1.75f);
			Projectile WeaponProj = new Projectile(WeaponData, Player.Model.LeftHandPosition + Player.Model.Human.Orientation * .5f +
			                                      Vector3.UnitY * 2f, Direction, Human);
			WeaponProj.RotateOnX = true;
			WeaponProj.Speed = 6.0f;
			WeaponProj.Lifetime = 5f;
			WeaponProj.HitEventHandler += delegate(Projectile Sender, Entity Hit) { 
				float Exp;
				Hit.Damage(30f * base.Level * .5f, Human, out Exp, true);
				Human.XP += Exp;
				if(KnockChance != -1){
					if(Utils.Rng.Next(0, KnockChance) == 0)
						Hit.KnockForSeconds(3);
				}
			};
			Sound.SoundManager.PlaySound(Sound.SoundType.BowSound, Human.Position, false,  1f + Utils.Rng.NextFloat() * .2f - .1f, 2.5f);
		}
		
		public override void KeyDown(){
			base.MaxCooldown = 8.5f - Math.Min(5f, base.Level * .5f);
			Player.IsCasting = true;
			Casting = true;
			Player.IsAttacking = true;
			Player.Model.LeftWeapon.InAttackStance = false;
			Player.Model.Model.Animator.StopBlend();
			Player.Model.Model.PlayAnimation(ThrowAnimation);
			Player.Movement.OrientatePlayer(Player);
		}
		
		public override void Update(){
			if(Player.IsCasting && Casting){
				
				World.WorldParticles.Color = new Vector4(1,1,1,1);
				World.WorldParticles.ParticleLifetime = 1f;
				World.WorldParticles.GravityEffect = .0f;
				World.WorldParticles.Direction = Vector3.Zero;
				World.WorldParticles.Scale = new Vector3(.25f,.25f,.25f);
				World.WorldParticles.Position = Player.Model.Model.TransformFromJoint(Player.Model.Model.JointDefaultPosition(Player.Model.LeftHand)
				                                                                             + Vector3.UnitZ *0f, Player.Model.LeftHand);
				World.WorldParticles.PositionErrorMargin = Vector3.One * 0.75f;
				
				for(int i = 0; i < 1; i++)
					World.WorldParticles.Emit();
			}
		}
		
		public override string Description {
			get {
				return "Throw a shuriken at your foes.";
			}
		}
	}
}