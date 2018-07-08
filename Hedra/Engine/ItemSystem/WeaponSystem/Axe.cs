﻿/*
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
	internal class Axe : MeleeWeapon
	{
        public Axe(VertexData Contents) : base(Contents)
		{
			AttackStanceAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSmash-Stance.dae");

		    PrimaryAnimations = new Animation[2];
		    PrimaryAnimations[0] = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSlash-Right.dae");
		    PrimaryAnimations[1] = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSlash-Left.dae");

		    for (int i = 0; i < PrimaryAnimations.Length; i++)
		    {
		        PrimaryAnimations[i].Loop = false;
		        PrimaryAnimations[i].Speed = 1.25f;
		        PrimaryAnimations[i].OnAnimationMid += delegate
		        {
		            Owner.Attack(Owner.DamageEquation);
		        };

		        PrimaryAnimations[i].OnAnimationEnd += delegate
		        {
		            Trail.Emit = false;
		        };
		    }
            SecondaryAnimations = new Animation[1];
		    SecondaryAnimations[0] = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSlash-Front.dae");
		    for (int i = 0; i < SecondaryAnimations.Length; i++)
		    {
		        SecondaryAnimations[i].Loop = false;
		        SecondaryAnimations[i].Speed = 1.5f;
                SecondaryAnimations[i].OnAnimationEnd += delegate
		        {
		            Owner.Attack(Owner.DamageEquation * 1.75f, delegate (Entity mob)
		            {
		                if (Utils.Rng.Next(0, 5) == 1)
		                    mob.KnockForSeconds(0.75f + Utils.Rng.NextFloat() * 1f);
		            });

		        };

		        SecondaryAnimations[i].OnAnimationEnd += delegate
		        {
		            Trail.Emit = false;
		        };
		    }
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