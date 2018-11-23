/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 08/05/2016
 * Time: 06:19 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Rendering;

namespace Hedra.WeaponSystem
{
    /// <summary>
    /// Description of TwoHandedSword.
    /// </summary>
    public class Axe : HeavyMeleeWeapon
    {        
        public Axe(VertexData Contents) : base(Contents)
        {
        }

        
        protected override void OnSecondaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            if(Type != AttackEventType.End) return;
            Owner.AttackSurroundings(Owner.DamageEquation * 1.75f * Options.DamageModifier, delegate (IEntity Mob)
            {
                if (Utils.Rng.Next(0, 5) == 1 && Options.Charge > .5f)
                    Mob.KnockForSeconds(0.75f + Utils.Rng.NextFloat() * 1f);
            });
        }
    }
}