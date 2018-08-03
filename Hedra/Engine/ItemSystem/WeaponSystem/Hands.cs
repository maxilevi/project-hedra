﻿/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 06/05/2016
 * Time: 12:49 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using OpenTK;

namespace Hedra.Engine.ItemSystem.WeaponSystem
{
	/// <summary>
	/// Description of Hands.
	/// </summary>
	public class Hands : MeleeWeapon
	{
		protected override bool ShouldPlaySound => false;
	    protected override string AttackStanceName => "Assets/Chr/WarriorPunch-Stance.dae";
	    protected override float PrimarySpeed => 1.0f;
	    protected override string[] PrimaryAnimationsNames => new []
	    {
		    "Assets/Chr/WarriorPunch.dae"
	    };
	    protected override float SecondarySpeed => 0.9f;
	    protected override string[] SecondaryAnimationsNames => new string[0];

        public Hands() : base(null)
		{
		}

	    protected override void OnPrimaryAttackEvent(AttackEventType Type, AttackOptions Options)
	    {
		    if(Type != AttackEventType.Mid) return;
		    Owner.Attack(Owner.DamageEquation * 0.75f);
	    }

	    protected override void OnSecondaryAttackEvent(AttackEventType Type, AttackOptions Options)
	    {
		    throw new System.NotImplementedException();
	    }

	    public override void Attack2(Humanoid Human)
	    {
	    }
	}
}
