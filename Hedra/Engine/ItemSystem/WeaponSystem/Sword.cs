/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 08/05/2016
 * Time: 06:19 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.ItemSystem.WeaponSystem
{
	/// <summary>
	/// Description of TwoHandedSword.
	/// </summary>
	public class Sword : MeleeWeapon
    {
	    private bool FrontSlash => PrimaryAnimationsIndex == 2;
	    private readonly float _swordHeight;
	    
	    protected override string AttackStanceName => "Assets/Chr/WarriorSmash-Stance.dae";
	    protected override float PrimarySpeed => 1.25f;
	    protected override string[] PrimaryAnimationsNames => new []
	    {
		    "Assets/Chr/WarriorSlash-Left.dae",
		    "Assets/Chr/WarriorSlash-Right.dae",
		    "Assets/Chr/WarriorSlash-Front.dae"
	    };
	    protected override float SecondarySpeed => 1.65f;
	    protected override string[] SecondaryAnimationsNames => new []
	    {
		    "Assets/Chr/WarriorLunge.dae"
	    };

        public Sword(VertexData Contents) : base(Contents)
        {
		}

	    protected override void OnPrimaryAttackEvent(AttackEventType Type, AttackOptions Options)
	    {
		    if(Type != AttackEventType.Mid) return;
		    Owner.Attack(Owner.DamageEquation * (FrontSlash ? 1.25f : 1.0f) );
	    }

	    protected override void OnSecondaryAttackEvent(AttackEventType Type, AttackOptions Options)
	    {
		    if(Type == AttackEventType.Start) return;
			Owner.Attack(Owner.DamageEquation * 1.10f * Options.DamageModifier, delegate(Entity Mob)
			{
				if (Utils.Rng.Next(0, 3) == 1 && Options.DamageModifier > .75f)
					Mob.KnockForSeconds(1.0f + Utils.Rng.NextFloat() * 2f);

				if (Utils.Rng.Next(0, 3) == 1 && Options.DamageModifier > .5f)
					Mob.AddComponent(new BleedingComponent(Mob, this.Owner, 4f,
						Owner.DamageEquation * 2f));
			});	    
	    }
		
        public override int ParsePrimaryIndex(int AnimationIndex)
	    {
	        return AnimationIndex == 5 ? 2 : AnimationIndex & 1;
	    }
		
		public override void Attack1(Humanoid Human, AttackOptions Options)
        {
			if(!base.MeetsRequirements()) return;

		    if (PrimaryAnimationsIndex == 5)
		        PrimaryAnimationsIndex = 0;

		    PrimaryAnimationsIndex++;

            base.BasePrimaryAttack(Human, Options);
		    Trail.Emit = false;
		    TaskManager.After(200, () => Trail.Emit = true);
		}

	    public override void Attack2(Humanoid Human, AttackOptions Options)
	    {
	        Options.IdleMovespeed *= Options.Charge * 2.5f + .25f;
	        Options.RunMovespeed = Options.Charge;
		    Options.DamageModifier *= Options.Charge;
            base.Attack2(Human, Options);
	    }
	}
}