/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 08/05/2016
 * Time: 06:19 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.WeaponSystem
{
    /// <summary>
    /// Description of TwoHandedSword.
    /// </summary>
    public class Hammer : HeavyMeleeWeapon
    {
        public override uint PrimaryAttackIcon => WeaponIcons.HammerPrimaryAttack;     
        public override uint SecondaryAttackIcon => WeaponIcons.HammerSecondaryAttack;
        protected override float SecondarySpeed => 1.5f;
        
        public Hammer(VertexData Contents) : base(Contents)
        {
        }

        protected override void OnSecondaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            if(Type != AttackEventType.End) return;
            Owner.AttackSurroundings(Owner.DamageEquation * 1.25f * Options.DamageModifier, Options.IgnoreEntities, delegate (IEntity Mob)
            {
                if (Utils.Rng.Next(1, 3) == 1 && Options.Charge > .5f)
                    Mob.KnockForSeconds(1.5f + Utils.Rng.NextFloat() * 2f);
            });
        }
        
        protected override Vector3 SheathedOffset => Vector3.Zero;
    }
}