using System;
using System.Linq;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ModuleSystem.Templates;
using Hedra.EntitySystem;
using Hedra.Numerics;

namespace Hedra.AISystem.Behaviours
{
    public class AdaptiveAttackBehaviour : AttackBehaviour
    {
        private readonly AttackObject[] _attacks;
        private readonly Timer _cooldownTimer;

        public AdaptiveAttackBehaviour(IEntity Parent, AttackAnimationTemplate[] Templates) : base(Parent)
        {
            if (!(Parent.Model is QuadrupedModel))
                throw new ArgumentException("Adaptive attack behaviour can only be used with quadruped models.");
            _attacks = CreateAttacks(Templates);
            _cooldownTimer = new Timer(Parent.AttackCooldown)
            {
                AutoReset = false
            };
        }

        private bool HasTarget => Target != null;

        public override void Update()
        {
            HandleFollowing();
            var availableAttacks = HasTarget
                ? _attacks.Where(T => !T.InCooldown && Parent.IsNear(Target, T.Range)).ToArray()
                : null;
            if (!Parent.Model.IsAttacking && HasTarget && Parent.Distance(Target) > GetLowestRange(availableAttacks))
                Follow.Update();
            else if (!Parent.Model.IsAttacking && HasTarget) Parent.LookAt(Target);
            if (CanAttack(availableAttacks))
            {
                FollowTimer.Reset();
                SelectAndDoAttack(availableAttacks);
            }

            UpdateCooldowns();
        }

        private float GetLowestRange(AttackObject[] Attacks)
        {
            if (Attacks.Length == 0) return 24;
            return Attacks.Min(A => A.Range);
        }

        private bool CanAttack(AttackObject[] Attacks)
        {
            if (!HasTarget) return false;
            return Attacks.Any(T => !T.InCooldown && Parent.IsNear(Target, T.Range)) && _cooldownTimer.Ready;
        }

        private void SelectAndDoAttack(AttackObject[] AvailableAttacks)
        {
            AvailableAttacks.Shuffle(Utils.Rng);
            var rng = Utils.Rng.NextFloat();
            for (var i = 0; i < AvailableAttacks.Length; ++i)
            {
                if (rng < AvailableAttacks[i].Chance)
                {
                    DoAttack(Array.IndexOf(_attacks, AvailableAttacks[i]));
                    break;
                }

                rng -= AvailableAttacks[i].Chance;
            }
        }

        private void DoAttack(int Index)
        {
            var asQuadruped = (QuadrupedModel)Parent.Model;
            asQuadruped.Attack(asQuadruped.AttackAnimations[Index], _attacks[Index].Range);
            _attacks[Index].ResetCooldown();
            _cooldownTimer.Reset();
        }

        private static AttackObject[] CreateAttacks(params AttackAnimationTemplate[] Templates)
        {
            var attacks = new AttackObject[Templates.Length];
            for (var i = 0; i < Templates.Length; ++i)
                attacks[i] = new AttackObject
                {
                    Chance = Templates[i].Chance,
                    MaxCooldown = Templates[i].Cooldown,
                    Range = Templates[i].Range
                };
            return attacks;
        }

        private void UpdateCooldowns()
        {
            _cooldownTimer.Tick();
            for (var i = 0; i < _attacks.Length; ++i) _attacks[i].Cooldown -= Time.Frametime;
        }

        private class AttackObject
        {
            public float Chance { get; set; }
            public float Range { get; set; }
            public float MaxCooldown { get; set; }
            public float Cooldown { get; set; }
            public bool InCooldown => Cooldown > 0;

            public void ResetCooldown()
            {
                Cooldown = MaxCooldown;
            }
        }
    }
}