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
using OpenTK;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of WeaponThrow.
	/// </summary>
	public class WeaponThrow : Skill
	{
		private readonly Animation ThrowAnimation;
		
		public WeaponThrow(Vector2 Position, Vector2 Scale, Panel InPanel, LocalPlayer Player) : base(Position, Scale, InPanel, Player) {
			base.TexId = Graphics2D.LoadFromAssets("Assets/Skills/Throw.png");
			base.ManaCost = 35f;
			base.MaxCooldown = 8.5f;


            ThrowAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorThrow.dae");
			ThrowAnimation.Loop = false;
			ThrowAnimation.Speed = 1.5f;
            this.ThrowAnimation.OnAnimationMid += delegate(Animation Sender)
            {
                Sound.SoundManager.PlaySound(Sound.SoundType.SlashSound, Player.Position);
            };
			ThrowAnimation.OnAnimationEnd += delegate(Animation Sender) 
            {
				
				Player.IsCasting = false;
				Casting = false;
				Player.IsAttacking = false;
				Player.Model.LeftWeapon.LockWeapon = false;
				CoroutineManager.StartCoroutine(ThrowWeapon);
			};
		}
		
		public override bool MeetsRequirements(SkillsBar Bar, int CastingAbilityCount)
		{
			return base.MeetsRequirements(Bar, CastingAbilityCount) && !Player.IsMoving;
		}
		
		private void ShootWeapon(Humanoid Human, Vector3 Direction, int KnockChance = -1){
			VertexData WeaponData = Player.Model.LeftWeapon.MeshData.Clone();
			WeaponData.Scale(Vector3.One * 1.75f);
			Projectile WeaponProj = new Projectile(WeaponData, Player.Model.LeftHandPosition + Player.Model.Human.Orientation * 2 +
			                                      Vector3.UnitY * 2f, Direction, Human);
			WeaponProj.RotateOnX = true;
			WeaponProj.Speed = 6.0f;
			WeaponProj.Lifetime = 5f;
			WeaponProj.HitEventHandler += delegate(Projectile Sender, Entity Hit) { 
				float Exp;
				Hit.Damage(Human.DamageEquation * 3.2f, Human, out Exp, true);
				Human.XP += Exp;
				if(KnockChance != -1){
					if(Utils.Rng.Next(0, KnockChance) == 0)
						Hit.KnockForSeconds(3);
				}
			};
			Sound.SoundManager.PlaySound(Sound.SoundType.BowSound, Human.Position);
		}
		
		private IEnumerator ThrowWeapon(){
			Player.Model.DisableWeapon = true;
			float TimePassed = 0;
			this.ShootWeapon(Player, Player.View.CrossDirection.NormalizedFast(), 4);
			while(TimePassed < 5){
				TimePassed += (float) Engine.Time.FrameTimeSeconds;
				yield return null;
			}
			Player.Model.DisableWeapon = false;
		}
		
		public override void KeyDown(){
			base.MaxCooldown = 8.5f - base.Level * .5f;
			Player.IsCasting = true;
			Casting = true;
			Player.IsAttacking = true;
			Player.Model.LeftWeapon.InAttackStance = false;
			Player.Model.LeftWeapon.LockWeapon = true;
			Player.Model.Model.Animator.StopBlend();
			Player.Model.Model.PlayAnimation(ThrowAnimation);
		}
		
		public override void Update(){
			if(Player.IsCasting && Casting){
				Matrix4 Mat4 = Player.Model.LeftHandMatrix.ClearTranslation() * 
					Matrix4.CreateTranslation(-Player.Model.Position + (Player.Model.LeftHandPosition + Player.Model.RightHandPosition) * .5f);
				
				Player.Model.LeftWeapon.Mesh.TransformationMatrix = Mat4;
				Player.Movement.OrientatePlayer(Player);
				Player.Model.LeftWeapon.Mesh.Position = Player.Model.Position;
				Player.Model.LeftWeapon.Mesh.Rotation = Vector3.Zero;
				Player.Model.LeftWeapon.Mesh.TargetRotation = new Vector3(180,0,0);
				Player.Model.LeftWeapon.Mesh.RotationPoint = Vector3.Zero;
				Player.Model.LeftWeapon.Mesh.LocalRotation = Vector3.Zero;
				Player.Model.LeftWeapon.Mesh.LocalRotationPoint = Vector3.Zero;
				Player.Model.LeftWeapon.Mesh.LocalPosition = Vector3.Zero;
				Player.Model.LeftWeapon.Mesh.BeforeLocalRotation = Vector3.UnitY * -0.7f;
				Player.Model.LeftWeapon.Mesh.TargetPosition = Vector3.Zero;
				Player.Model.LeftWeapon.Mesh.AnimationPosition = Vector3.Zero;
			}
		}
		
		public override string Description {
			get {
				return "Throw your current weapon at your foes.";
			}
		}
	}
}