/*
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
	public class Hands : Weapon
	{
	    public override bool IsMelee { get; protected set; } = true;

	    public Hands() : base(null)
		{
		    AttackStanceAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorPunch-Stance.dae");

            PrimaryAnimations = new Animation[1];
            PrimaryAnimations[0] = AnimationLoader.LoadAnimation("Assets/Chr/WarriorPunch.dae");
		    PrimaryAnimations[0].Speed = 2.0f;
		    PrimaryAnimations[0].OnAnimationMid += delegate
		    {
		        Owner.Attack(Owner.DamageEquation * 0.75f);
		    };
            for (var i = 0; i < PrimaryAnimations.Length; i++)
		    {
		        PrimaryAnimations[i].Loop = false;
		    }

		    SecondaryAnimations = new Animation[0];

            base.ShouldPlaySound = false;
		}
		
		public override void Attack2(Humanoid Human){}
	}
}
