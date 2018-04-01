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
using Hedra.Engine.ItemSystem.WeaponSystem;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Player.ToolbarSystem;
using Hedra.Engine.Rendering.Animation;
using OpenTK;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of ArcherPoisonArrow.
	/// </summary>
	public class IceArrow : BaseSkill
	{
		private Animation ShootAnimation;
		private float BaseDamage = 35f, Damage;
		
		public IceArrow(Vector2 Position, Vector2 Scale, Panel InPanel, LocalPlayer Player) : base(Position, Scale, InPanel, Player) {
			base.TexId = Graphics2D.LoadFromAssets("Assets/Skills/IceArrow.png");
			base.ManaCost = 35f;
            base.MaxCooldown = 6.5f;
			
			ShootAnimation = AnimationLoader.LoadAnimation("Assets/Chr/ArcherTripleShoot.dae");
			ShootAnimation.Loop = false;
			ShootAnimation.OnAnimationMid += delegate {

				if(Player.Model.LeftWeapon is Bow){
					Bow PlayerBow = Player.Model.LeftWeapon as Bow;
					
					Projectile Arrow = PlayerBow.ShootArrow(Player, Player.View.CrossDirection);
					Arrow.MoveEventHandler += delegate { 
						Arrow.Mesh.Tint = Bar.Blue * new Vector4(1,1,3,1) * .7f;
					};
					Arrow.HitEventHandler += delegate(Projectile Sender, Entity Hit) {
						float Exp;
						Hit.Damage(Player.DamageEquation * 0.5f, Player, out Exp, true);
						Player.XP += Exp;
						
						Hit.AddComponent( new FreezingComponent(Hit, Player, 3 + Utils.Rng.NextFloat() * 2f, Damage) );
						
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
		
		public override bool MeetsRequirements(Toolbar Bar, int CastingAbilityCount)
		{
			return base.MeetsRequirements(Bar, CastingAbilityCount) && Player.Model.LeftWeapon is Bow;
		}
		
		public override void KeyDown()
		{
			this.Damage = BaseDamage + 7.5f * base.Level;
			base.MaxCooldown = 6.5f - base.Level * .5f;
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
