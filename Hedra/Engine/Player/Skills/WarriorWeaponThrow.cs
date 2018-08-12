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
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Player.ToolbarSystem;
using Hedra.Engine.Rendering.Animation;
using OpenTK;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of WeaponThrow.
	/// </summary>
	public class WeaponThrow : BaseSkill
	{
		private readonly Animation ThrowAnimation;
		
		public WeaponThrow() : base() {
			base.TextureId = Graphics2D.LoadFromAssets("Assets/Skills/Throw.png");
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
				Player.LeftWeapon.LockWeapon = false;
                Player.Model.LockWeapon = false;
                CoroutineManager.StartCoroutine(ThrowWeapon);
			};
		}
		
		public override bool MeetsRequirements(Toolbar Bar, int CastingAbilityCount)
		{
			return base.MeetsRequirements(Bar, CastingAbilityCount) && !Player.IsMoving && Player.MainWeapon != null;
		}
		
		private void ShootWeapon(IHumanoid Human, Vector3 Direction, int KnockChance = -1){
			var weaponData = Player.LeftWeapon.MeshData.Clone();
		    var startingPosition = Player.Model.LeftWeaponPosition + Player.Model.Human.Orientation * 2 +
		                           Vector3.UnitY * 2f;

            var weaponProj = new Projectile(Human, startingPosition, weaponData)
		    {
                Propulsion = Direction * 2f,
		        Lifetime = 5f
		    };
		    weaponProj.HitEventHandler += delegate(Projectile Sender, IEntity Hit) {
		        Hit.Damage(Human.DamageEquation * .5f, Human, out float Exp, true);
				Human.XP += Exp;
				if(KnockChance != -1){
					if(Utils.Rng.Next(0, KnockChance) == 0)
						Hit.KnockForSeconds(3);
				}
			};
			Sound.SoundManager.PlaySound(Sound.SoundType.BowSound, Human.Position);
		}
		
		private IEnumerator ThrowWeapon(){
			Player.Toolbar.DisableAttack = true;
			float timePassed = 0;
			this.ShootWeapon(Player, Player.View.CrossDirection.NormalizedFast(), 4);
			while(timePassed < 5){
				timePassed += Time.IndependantDeltaTime;
				yield return null;
			}
			Player.Toolbar.DisableAttack = false;
		    for (var i = 0; i < Player.LeftWeapon.Meshes.Length; i++)
		    {
		        Player.LeftWeapon.Meshes[i].Enabled = true;
		    }
        }
		
		public override void Use(){
			base.MaxCooldown = Math.Max(5, 8.5f - base.Level * .1f);
			Player.IsCasting = true;
			Casting = true;
			Player.IsAttacking = true;
			Player.LeftWeapon.InAttackStance = false;
			Player.LeftWeapon.LockWeapon = true;
		    Player.Model.LockWeapon = false;
			Player.Model.PlayAnimation(ThrowAnimation);
		    for (var i = 0; i < Player.LeftWeapon.Meshes.Length; i++)
		    {
		        Player.LeftWeapon.Meshes[i].Enabled = false;
		    }
		}
		
		public override void Update(){
			if(Player.IsCasting && Casting){
				Matrix4 Mat4 = Player.Model.LeftWeaponMatrix.ClearTranslation() * 
					Matrix4.CreateTranslation(-Player.Model.Position + (Player.Model.LeftWeaponPosition + Player.Model.RightWeaponPosition) * .5f);
				
				Player.LeftWeapon.MainMesh.TransformationMatrix = Matrix4.Identity;
				Player.Movement.Orientate();
				Player.LeftWeapon.MainMesh.Position = Player.Model.Position;
				Player.LeftWeapon.MainMesh.Rotation = Vector3.Zero;
				Player.LeftWeapon.MainMesh.TargetRotation = new Vector3(180,0,0);
				Player.LeftWeapon.MainMesh.RotationPoint = Vector3.Zero;
				Player.LeftWeapon.MainMesh.LocalRotation = Vector3.Zero;
				Player.LeftWeapon.MainMesh.LocalRotationPoint = Vector3.Zero;
				Player.LeftWeapon.MainMesh.LocalPosition = Vector3.Zero;
				Player.LeftWeapon.MainMesh.BeforeLocalRotation = Vector3.UnitY * -0.7f;
				Player.LeftWeapon.MainMesh.TargetPosition = Vector3.Zero;
				Player.LeftWeapon.MainMesh.AnimationPosition = Vector3.Zero;
			}
		}
		
		public override string Description => "Throw your current weapon at your foes.";
	}
}