/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 07/06/2016
 * Time: 07:29 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hedra.AISystem;
using Hedra.Components.Effects;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Numerics;
using Hedra.Rendering.UI;
using Hedra.Sound;
using SixLabors.ImageSharp;

namespace Hedra.Components
{
    /// <summary>
    ///     Description of DamageComponent.
    /// </summary>
    public delegate void OnDamageEventHandler(DamageEventArgs Args);

    public delegate void OnDeadEvent(DeadEventArgs Args);

    public class DamageComponent : SingularComponent<DamageComponent, IEntity>
    {
        public const float DefaultMissChance = 0.075f;
        private static readonly Vector4 DamageTint = new Vector4(2.0f, 0.1f, 0.1f, 1);
        private static readonly Vector4 DamageTint2 = Vector4.One;
        private readonly List<BaseBillboard> _damageLabels;
        private readonly List<Predicate<IEntity>> _ignoreList;
        private float _attackedTimer;
        private Vector4 _targetTint;
        private float _tintStrength;
        private float _tintTimer;
        private bool _wasDead;

        public DamageComponent(IEntity Parent) : base(Parent)
        {
            _damageLabels = new List<BaseBillboard>();
            _ignoreList = new List<Predicate<IEntity>>();
            OnDeadEvent += A => _wasDead = true;
            OnDamageEvent += A =>
            {
                if (!_wasDead && Parent.IsDead)
                {
                    var effectComponent = Parent.SearchComponent<DamagingEffectComponent>();
                    OnDeadEvent?.Invoke(new DeadEventArgs(A.Victim, A.Damager, A.Amount, A.Experience, A.DamageType));
                }
            };
        }

        public float XpToGive { get; set; } = 8;
        public bool Immune { get; set; }
        public bool Delete { get; set; } = true;
        public bool PlayDeleteAnimation { get; set; } = true;
        public bool PushOnHit { get; set; } = true;
        public bool AICanReach { get; set; } = true;
        public float MissChance { get; set; } = DefaultMissChance;

        public BaseBillboard[] Labels => _damageLabels.ToArray();

        /// <summary>
        ///     Returns a bool representing if the Entity has been attacked in the last six seconds.
        /// </summary>
        public bool HasBeenAttacked { get; private set; }

        public float XPObtained => XpToGive * Balancer.XPObtainedMultiplier;
        public event OnDamageEventHandler OnDamageEvent;
        public event OnDeadEvent OnDeadEvent;

        public override void Update()
        {
            if ((Parent.Model.Tint - _targetTint).LengthFast() > 0.005f)
                Parent.Model.Tint = Mathf.Lerp(Parent.Model.Tint, _targetTint, Time.DeltaTime * 8f);

            _tintTimer -= Time.IndependentDeltaTime;
            _tintTimer = Math.Max(_tintTimer, 0);

            if (Math.Abs(_tintTimer) < 0.005f) _targetTint = Vector4.One;

            if (HasBeenAttacked)
            {
                _attackedTimer -= Time.IndependentDeltaTime;
                if (_attackedTimer < 0) HasBeenAttacked = false;
            }

            for (var i = _damageLabels.Count - 1; i > -1; i--)
                if (_damageLabels[i].Disposed)
                    _damageLabels.RemoveAt(i);
        }

        private void TriggerTint(float Strength)
        {
            _tintTimer = 0.25f;
            _tintStrength = Math.Max(Math.Min(1f, Strength * 15f), 0f);
            _targetTint = Mathf.Lerp(DamageTint * 8f, Vector4.One, 1f - _tintStrength);
            Parent.Model.Tint = _targetTint;
        }

        public void Damage(float Amount, IEntity Damager, out float Exp, bool PlaySound, bool PushBack)
        {
            Damage(Amount, Damager, out Exp, PlaySound, PushBack, DamageType.Unknown);
        }

        public void Damage(float Amount, IEntity Damager, out float Exp, bool PlaySound, bool PushBack,
            DamageType DamageType)
        {
            Damage(Amount, Damager, out Exp, out _, PlaySound, PushBack, DamageType);
        }

        public void Damage(float Amount, IEntity Damager, out float Exp, out float Inflicted, bool PlaySound,
            bool PushBack, DamageType DamageType)
        {
            Exp = 0;
            Inflicted = 0;

            var armor = Parent is IHumanoid human ? human.Armor : 0;
            Amount *= 1.0f - armor / 100f;
            Amount *= 1.0f / Parent.AttackResistance;
            Amount *= Parent.IsUndead ? Damager?.Attributes.UndeadDamageModifier ?? 1 : 1;
            if (Parent.IsDead || Damager != null && _ignoreList.Any(I => I.Invoke(Damager))) return;

            var accuracy = Parent is IHumanoid damagerHuman ? damagerHuman.Accuracy : 0;
            var scaledMissChance = MissChance * (1 - accuracy / 100f);
            var shouldMiss = Utils.Rng.NextFloat() < scaledMissChance;
            _attackedTimer = 6;
            HasBeenAttacked = true;
            var isImmune = Immune; // || (Parent.IsStuck && !Parent.Model.Pause) || !AICanReach;

            if (!Parent.IsStatic && PlaySound &&
                (GameManager.Player.Position - Parent.Position).LengthSquared() < 80 * 80 && Amount >= 1f)
            {
                var asHuman = Damager as Humanoid;
                var baseDamage = Damager != null
                    ? asHuman?.BaseDamageEquation
                      ?? (Damager.SearchComponent<BasicAIComponent>() != null ? Damager.AttackDamage : Amount)
                    : Amount / 3f;
                var color = Color.White;
                var dmgDiff = Amount / baseDamage;
                if (dmgDiff > 1.85f) color = Color.Gold;
                if (dmgDiff > 2.25f) color = Color.Red;
                if (isImmune || shouldMiss) color = Color.White;
                var maxSize = float.MaxValue;
                if (Parent is LocalPlayer) color = Color.Red;
                var font = FontCache.GetBold(Math.Min(12 + 6 * dmgDiff, maxSize));
                var dmgString = ((int)Amount).ToString();
                var missString = isImmune ? "IMMUNE" : "MISS";
                var dmgLabel = new TextBillboard(1.8f, !isImmune && !shouldMiss ? dmgString : missString, color,
                    font, () => Parent.Position)
                {
                    Vanish = true,
                    VanishSpeed = 4
                };
                _damageLabels.Add(dmgLabel);
            }

            Exp = 0;

            if (PlaySound)
                SoundPlayer.PlaySoundWithVariation(!shouldMiss ? SoundType.HitSound : SoundType.SlashSound,
                    Parent.Position);

            if (shouldMiss || isImmune) return;

            TriggerTint(Amount / Parent.Health);
            Inflicted = Amount;
            Parent.Health = Math.Max(Parent.Health - Amount, 0);
            if (Damager != null && Damager != Parent && PushBack
                && Parent.Size.LengthFast() < Damager.Size.LengthFast())
            {
                var direction = (Damager.Position - Parent.Position).Normalized();
                var factor = 0.5f;
                var averageSize = (Parent.Size.X + Parent.Size.Z) * .5f;
                if (PushOnHit)
                    Parent.Physics.ApplyImpulse(-direction * factor * averageSize);
            }

            if (Parent.Health <= 0 && !Parent.IsDead)
            {
                Parent.IsDead = true;
                Parent.Physics.CollidesWithEntities = false;
                Exp = XPObtained;
                if (PlayDeleteAnimation)
                    RoutineManager.StartRoutine(DisposeCoroutine);
            }

            if (OnDamageEvent != null && Math.Abs(Amount) > 0.005f)
                OnDamageEvent.Invoke(new DamageEventArgs(Parent, Damager, Amount, Exp, DamageType));
        }


        public IEnumerator DisposeCoroutine()
        {
            float currentTime = 0;

            if (Parent.Model is IDisposeAnimation animable)
            {
                SoundPlayer.PlaySound(SoundType.GlassBreak, Parent.Position);
                animable.DisposeAnimation();

                while (currentTime < 4)
                {
                    currentTime += Time.IndependentDeltaTime * 2.0f;
                    animable.DisposeTime = currentTime;
                    yield return null;
                }
            }
            else
            {
                while (currentTime < 4)
                {
                    currentTime += Time.IndependentDeltaTime * 1f;
                    Parent.Model.Alpha = 1.0f - currentTime / 4.0f;
                    yield return null;
                }
            }

            if (Delete)
                Parent.Dispose();
        }

        public void Ignore(Predicate<IEntity> Predicate)
        {
            _ignoreList.Add(Predicate);
        }

        public bool HasIgnoreFor(IEntity Entity)
        {
            return _ignoreList.Any(P => P(Entity));
        }
    }

    public class DamageEventArgs : EventArgs
    {
        public DamageEventArgs(IEntity Victim, IEntity Damager, float Amount, float Experience, DamageType DamageType)
        {
            this.Victim = Victim;
            this.Damager = Damager;
            this.Amount = Amount;
            this.Experience = Experience;
            this.DamageType = DamageType;
        }

        public IEntity Victim { get; }
        public IEntity Damager { get; }
        public float Amount { get; }
        public float Experience { get; }
        public DamageType DamageType { get; }
    }

    public class DeadEventArgs : DamageEventArgs
    {
        public DeadEventArgs(IEntity Victim, IEntity Damager, float Amount, float Experience, DamageType DamageType) :
            base(Victim, Damager, Amount, Experience, DamageType)
        {
        }
    }
}