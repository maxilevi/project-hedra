/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 08/05/2016
 * Time: 06:19 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

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
	internal class Sword : MeleeWeapon
    {
	    private bool FrontSlash => PrimaryAnimationsIndex == 2;
	    private readonly float _swordHeight;

        public Sword(VertexData Contents) : base(Contents)
        {
            AttackStanceAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSmash-Stance.dae");

		    PrimaryAnimations = new Animation[3];
		    PrimaryAnimations[0] = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSlash-Left.dae");
		    PrimaryAnimations[1] = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSlash-Right.dae");
		    PrimaryAnimations[2] = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSlash-Front.dae");

            for (var i = 0; i < PrimaryAnimations.Length; i++)
		    {
		        PrimaryAnimations[i].Loop = false;
		        PrimaryAnimations[i].Speed = 1.25f;
		        PrimaryAnimations[i].OnAnimationMid += delegate
		        {
		            Owner.Attack(Owner.DamageEquation * (FrontSlash ? 1.25f : 1.0f) );
		            Trail.Emit = false;
                };
		    }

		    SecondaryAnimations = new Animation[1];
            SecondaryAnimations[0] = AnimationLoader.LoadAnimation("Assets/Chr/WarriorLunge.dae");

            for (var k = 0; k < SecondaryAnimations.Length; k++)
		    {
		        SecondaryAnimations[k].Loop = false;
		        SecondaryAnimations[k].Speed = 1.65f;
                SecondaryAnimations[k].OnAnimationEnd += delegate
		        {
		            Owner.Attack(Owner.DamageEquation * 1.10f, delegate(Entity Mob)
		            {

		                if (Utils.Rng.Next(0, 3) == 1)
		                    Mob.KnockForSeconds(1.0f + Utils.Rng.NextFloat() * 2f);

		                if (Utils.Rng.Next(0, 3) == 1)
		                    Mob.AddComponent(new BleedingComponent(Mob, this.Owner, 4f,
		                        Owner.DamageEquation * 2f));

		            });
		        };
		    }
		}
		
        public override int ParsePrimaryIndex(int AnimationIndex)
	    {
	        return AnimationIndex == 5 ? 2 : AnimationIndex & 1;
	    }
		
		public override void Attack1(Humanoid Human){
			if(!base.MeetsRequirements()) return;

		    if (PrimaryAnimationsIndex == 5)
		        PrimaryAnimationsIndex = 0;

		    PrimaryAnimationsIndex++;

            base.BasePrimaryAttack(Human);
		    Trail.Emit = false;
		    TaskManager.After(200, () => Trail.Emit = true);
		}
	}
}