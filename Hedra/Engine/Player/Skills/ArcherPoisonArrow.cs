/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 30/04/2017
 * Time: 10:22 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ItemSystem.WeaponSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.Player.Skills
{
	/// <summary>
	/// Description of ArcherPoisonArrow.
	/// </summary>
	public class ArcherPoisonArrow : SpecialAttackSkill<Bow>
	{
		private const float BaseDamage = 80f;
		private const float BaseCooldown = 14f;
		private const float CooldownCap = 6f;
		private const float BaseManaCost = 30f;
		public override uint TextureId => Graphics2D.LoadFromAssets("Assets/Skills/PoisonArrow.png");
		public override string Description => "Shoot a poisonous arrow.";
		private float Damage => BaseDamage * (base.Level * 0.40f) + BaseDamage;
		public override float MaxCooldown => Math.Max(BaseCooldown - 0.80f * base.Level, CooldownCap);
		public override float ManaCost => BaseManaCost;

		protected override void BeforeUse(Bow Weapon)
		{
			void HandlerLambda(Projectile A) => ModifierHandler(Weapon, A, HandlerLambda);
			Weapon.BowModifiers += HandlerLambda;
		}

		private void ModifierHandler(Bow Weapon, Projectile Arrow, OnModifyArrowEvent Event)
		{
			Arrow.MoveEventHandler += Sender =>
			{
				Arrow.Mesh.Tint = Colors.PoisonGreen * new Vector4(1,3,1,1);
			};
			Arrow.HitEventHandler += delegate(Projectile Sender, IEntity Hit)
			{				
				Hit.AddComponent( new PoisonComponent(Hit, Player, 3 + Utils.Rng.NextFloat() * 2f, Damage) );
			};
			Weapon.BowModifiers -= Event;
		}
	}
}
