/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 08/05/2016
 * Time: 06:19 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Numerics;
using Hedra.EntitySystem;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.WeaponSystem
{
    /// <summary>
    ///     Description of TwoHandedSword.
    /// </summary>
    public class Axe : HeavyMeleeWeapon
    {
        public Axe(VertexData Contents) : base(Contents)
        {
        }

        public override uint PrimaryAttackIcon => WeaponIcons.AxePrimaryAttack;
        public override uint SecondaryAttackIcon => WeaponIcons.AxeSecondaryAttack;

        protected override Vector3 SheathedOffset => Vector3.Zero;

        protected override void OnSecondaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            if (Type != AttackEventType.End) return;
            Owner.AttackSurroundings(Owner.DamageEquation * 1.75f * Options.DamageModifier, Options.IgnoreEntities,
                delegate(IEntity Mob)
                {
                    if (Utils.Rng.Next(0, 5) == 1 && Options.Charge > .5f)
                        Mob.KnockForSeconds(0.75f + Utils.Rng.NextFloat() * 1f);
                });
        }
    }
}