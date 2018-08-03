/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 08/05/2016
 * Time: 06:19 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using OpenTK;

namespace Hedra.Engine.ItemSystem.WeaponSystem
{
	/// <summary>
	/// Description of TwoHandedSword.
	/// </summary>
	public class Axe : MeleeWeapon
	{		
		protected override string AttackStanceName => "Assets/Chr/WarriorSmash-Stance.dae";
		protected override float PrimarySpeed => 1.15f;
		protected override string[] PrimaryAnimationsNames => new []
		{
			"Assets/Chr/WarriorSlash-Right.dae",
			"Assets/Chr/WarriorSlash-Left.dae"
		};
		protected override float SecondarySpeed => 1.25f;
		protected override string[] SecondaryAnimationsNames => new []
		{
			"Assets/Chr/WarriorSlash-Front.dae"
		};
		
		public Axe(VertexData Contents) : base(Contents)
		{
		}
		
		protected override void OnPrimaryAttackEvent(AttackEventType Type, AttackOptions Options)
		{
			if(AttackEventType.Mid != Type) return;
			Owner.Attack(Owner.DamageEquation);
		}
		
		protected override void OnSecondaryAttackEvent(AttackEventType Type, AttackOptions Options)
		{
			if(Type != AttackEventType.End) return;
			Owner.Attack(Owner.DamageEquation * 1.75f * Options.DamageModifier, delegate (Entity Mob)
			{
				if (Utils.Rng.Next(0, 5) == 1 && Options.Charge > .5f)
					Mob.KnockForSeconds(0.75f + Utils.Rng.NextFloat() * 1f);
			});
		}

	    public override void Attack1(Humanoid Human)
	    {
	        if (!base.MeetsRequirements()) return;

	        base.Attack1(Human);

	        TaskManager.After(250, () => Trail.Emit = true);
	    }

        public override void Attack2(Humanoid Human)
        {
		    if (!base.MeetsRequirements()) return;

		    base.Attack2(Human);

		    TaskManager.After(200, () => Trail.Emit = true);

        }
	}
}