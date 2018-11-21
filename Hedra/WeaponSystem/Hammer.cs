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

namespace Hedra.WeaponSystem
{
    /// <summary>
    /// Description of TwoHandedSword.
    /// </summary>
    public class Hammer : MeleeWeapon
    {
        public Hammer(VertexData Contents) : base(Contents)
        {
        }
        
        protected override string AttackStanceName => "Assets/Chr/WarriorSmash-Stance.dae";
        protected override float PrimarySpeed => 1.15f;
        protected override string[] PrimaryAnimationsNames => new []
        {
            "Assets/Chr/WarriorSlash-Right.dae",
            "Assets/Chr/WarriorSlash-Left.dae"
        };
        protected override float SecondarySpeed => 1.5f;
        protected override string[] SecondaryAnimationsNames => new []
        {
            "Assets/Chr/WarriorSlash-Front.dae"
        };
        
        protected override void OnPrimaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            if(AttackEventType.Mid != Type) return;
            Owner.AttackSurroundings(Owner.DamageEquation);
        }
        
        protected override void OnSecondaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            if(Type != AttackEventType.End) return;
            Owner.AttackSurroundings(Owner.DamageEquation * 1.25f * Options.DamageModifier, delegate (Entity Mob)
            {
                if (Utils.Rng.Next(1, 3) == 1 && Options.Charge > .5f)
                    Mob.KnockForSeconds(1.5f + Utils.Rng.NextFloat() * 2f);
            });
        }
                
        public override void Attack1(IHumanoid Human, AttackOptions Options)
        {
            if (!base.MeetsRequirements()) return;

            base.Attack1(Human, Options);

            TaskManager.After(250, () => Trail.Emit = true);
        }
        
        public override void Attack2(IHumanoid Human, AttackOptions Options)
        {
            if (!base.MeetsRequirements()) return;

            base.Attack2(Human, Options);

            TaskManager.After(200, () => Trail.Emit = true);

        }
    }
}