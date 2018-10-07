﻿/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:14 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ItemSystem.WeaponSystem;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.Player.Skills.Archer
{
	/// <summary>
	/// Description of Resistance.
	/// </summary>
	public class Puncture : SpecialAttackSkill<Bow>
	{
		public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/PierceArrows.png");

		protected override void BeforeUse(Bow Weapon)
		{
			void HandlerLambda(Projectile A) => PierceModifier(Weapon, A, HandlerLambda);
			Weapon.BowModifiers += HandlerLambda;
		}

		private void PierceModifier(Bow Weapon, Projectile ArrowProj, OnModifyArrowEvent Lambda)
		{
			ArrowProj.HitEventHandler += delegate(Projectile Sender, IEntity Hit)
			{
				if(Utils.Rng.Next(0, 5) == 0)
				{
					Hit.AddComponent( new BleedingComponent(Hit, Player, 3f, 20f) );
				}
			};
			Weapon.BowModifiers -= Lambda;
		}
		
		public override string Description => "Arrows have a high chance to cause bleeding.";
		public override string DisplayName => "Puncture";
	}
}