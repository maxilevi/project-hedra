 /*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 08/12/2016
 * Time: 11:13 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.Management;
using Hedra.EntitySystem;
using Hedra.WeaponSystem;
using System.Numerics;

namespace Hedra.AISystem.Humanoid
{
    /// <summary>
    /// Description of WarriorAI.
    /// </summary>
    public class MeleeAIComponent : CombatAIComponent
    {
        private float _attackTimer;
        protected override float SearchRadius => 64;
        protected override float AttackRadius => throw new NotImplementedException();
        protected override float ForgetRadius => 64;

        public MeleeAIComponent(IHumanoid Parent, bool IsFriendly) : base(Parent, IsFriendly)
        {
        }
        
        protected override void DoUpdate()
        {
            _attackTimer -= Time.DeltaTime;
        }

        protected override void OnAttack()
        {
            if (!(_attackTimer < 0)) return;
            if (Utils.Rng.Next(0, 7) == 1)
            {
                Parent.LeftWeapon.Attack2(Parent, new AttackOptions
                {
                    IgnoreEntities = IgnoreEntities
                });
            }
            else
            {
                Parent.LeftWeapon.Attack1(Parent, new AttackOptions
                {
                    IgnoreEntities = IgnoreEntities
                });
            }
            _attackTimer = 1.25f;
        }

        protected override bool InAttackRadius(IEntity Target)
        {
            return Parent.InAttackRange(Target, 1.5f);
        }
    }
}
