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
using OpenTK;
using System.Collections.Generic;
using Hedra.Engine.Generation;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of WeaponThrow.
	/// </summary>
	public class Whirlwind : Skill
	{
		private float RotationY = 0, PassedTime = 0;
		private Animation WhirlwindAnimation;
		private Dictionary<Entity, float> AffectedEntities = new Dictionary<Entity, float>();
	    private TrailRenderer _trail;
		
		public Whirlwind(Vector2 Position, Vector2 Scale, Panel InPanel, LocalPlayer Player) : base(Position, Scale, InPanel, Player) {
			base.TexId = Graphics2D.LoadFromAssets("Assets/Skills/Spin.png");
			base.ManaCost = 85;
			base.MaxCooldown = 8.5f;
            _trail = new TrailRenderer( () => LocalPlayer.Instance.MainWeapon.Weapon.WeaponTip, Vector4.One);
			
			WhirlwindAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorWhirlwind.dae");
			WhirlwindAnimation.OnAnimationEnd += delegate{
				if(PassedTime > 4){
					Player.Model.TargetRotation = Vector3.Zero;
					Player.Model.LeftWeapon.MainMesh.TargetRotation = Vector3.Zero;
					Player.Model.LeftWeapon.MainMesh.TargetRotation = Vector3.Zero;
					Player.Model.Model.Rotation = Vector3.Zero;
					Player.IsCasting = false;
					Casting = false;
					Player.IsAttacking = false;
					Player.Model.LeftWeapon.LockWeapon = false;
				    _trail.Emit = false;
				}
			};
		}

		public override bool MeetsRequirements(AbilityBarSystem.AbilityBar Bar, int CastingAbilityCount)
		{
            bool Met = !Player.AbilityBar.DisableAttack;
			return base.MeetsRequirements(Bar, CastingAbilityCount) && Met;
		}
		
		public override void KeyDown(){
			base.MaxCooldown = 8.5f - base.Level * .5f;
			Player.IsCasting = true;
			Casting = true;
			Player.IsAttacking = true;
			AffectedEntities.Clear();
			PassedTime = 0;
			Player.Model.LeftWeapon.LockWeapon = true;
			Player.Model.Model.Animator.StopBlend();
			Player.Model.Model.PlayAnimation(WhirlwindAnimation);
			
			Player.Model.TargetRotation = Vector3.Zero;
			Player.Model.LeftWeapon.MainMesh.TargetRotation = Vector3.Zero;
			Player.Model.LeftWeapon.MainMesh.TargetRotation = Vector3.Zero;
			Player.Model.Model.Rotation = Vector3.Zero;
		    _trail.Emit = true;
		}
		
		float FrameCounter = 0;
		public override void Update(){			
			if(Player.IsCasting && Casting){
				if(Player.IsDead){
					Player.IsCasting = false;
					Casting = false;
					Player.IsAttacking = false;
					Player.Model.LeftWeapon.LockWeapon = false;
					return;
				}
				RotationY += Engine.Time.ScaledFrameTimeSeconds * 2000f;
                //if(RotationY > 720) 
			    //	RotationY -= 360;

			    Chunk underChunk = World.GetChunkAt(Player.Position);
                Player.Model.LeftWeapon.MainMesh.TransformationMatrix = Matrix4.Identity;
				Player.Model.TargetRotation = new Vector3(0,RotationY,0);
				Player.Model.LeftWeapon.MainMesh.Position = Player.Model.Position;
				Player.Model.LeftWeapon.MainMesh.Rotation = Vector3.Zero;
				Player.Model.LeftWeapon.MainMesh.TargetRotation = new Vector3(RotationY,0,0);
				Player.Model.LeftWeapon.MainMesh.RotationPoint = Player.Model.Position - (Player.Model.LeftHandPosition + Player.Model.RightHandPosition) / 2;
				Player.Model.LeftWeapon.MainMesh.LocalRotation = new Vector3(90,0,90);
				Player.Model.LeftWeapon.MainMesh.LocalRotationPoint = Vector3.Zero;
				Player.Model.LeftWeapon.MainMesh.LocalPosition = (Player.Model.LeftHandPosition + Player.Model.RightHandPosition) / 2 - Player.Model.LeftWeapon.MainMesh.Position;
				Player.Model.LeftWeapon.MainMesh.BeforeLocalRotation = Vector3.Zero;
				Player.Model.LeftWeapon.MainMesh.TargetPosition = Vector3.Zero;
				Player.Model.LeftWeapon.MainMesh.AnimationPosition = Vector3.Zero;
				
				Block B = World.GetHighestBlockAt( (int) Player.Position.X, (int) Player.Position.Z);
				World.WorldParticles.VariateUniformly = true;
				World.WorldParticles.Color = World.GetHighestBlockAt( (int) this.Player.Position.X, (int) this.Player.Position.Z).GetColor(underChunk.Biome.Colors);
				World.WorldParticles.Position = this.Player.Position - Vector3.UnitY;
				World.WorldParticles.Scale = Vector3.One * .15f;
				World.WorldParticles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);
				World.WorldParticles.Direction = (-this.Player.Orientation + Vector3.UnitY * 2.75f) * .15f;
				World.WorldParticles.ParticleLifetime = 1;
				World.WorldParticles.GravityEffect = .1f;
				World.WorldParticles.PositionErrorMargin = new Vector3(.75f, .75f, .75f);


				if(World.WorldParticles.Color == Block.GetColor(BlockType.Grass, underChunk.Biome.Colors))
					World.WorldParticles.Color = underChunk.Biome.Colors.GrassColor;
				
				for(int i = 0; i < 1; i++){
					World.WorldParticles.Emit();
				}
				
				for(int i = World.Entities.Count-1; i > 0; i--){
					if( (World.Entities[i].Position  - Player.Position).LengthFast <
					   (this.Player.DefaultBox.Max - this.Player.DefaultBox.Min).LengthFast +
					   (World.Entities[i].DefaultBox.Max - World.Entities[i].DefaultBox.Min).LengthFast - 2 &&
					    World.Entities[i] != Player && !World.Entities[i].IsStatic){
						
						float Dmg = Player.DamageEquation * Engine.Time.ScaledFrameTimeSeconds * 2f * (1+base.Level * .1f);
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
            _trail.Update();
		}
		
		public override string Description => "A fierce spinning attack.";
	}
}