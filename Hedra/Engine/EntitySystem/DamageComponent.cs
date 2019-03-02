/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 07/06/2016
 * Time: 07:29 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using OpenTK;
using System.Collections;
using System.Collections.Generic;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using System.Drawing;
using System.Linq;
using Hedra.AISystem;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.Player;
using Hedra.Engine.Sound;
using Hedra.EntitySystem;
using Hedra.Sound;

namespace Hedra.Engine.EntitySystem
{
    /// <summary>
    /// Description of DamageComponent.
    /// </summary>
    public delegate void OnDamageEventHandler(DamageEventArgs Args);

    public class DamageComponent : EntityComponent
    {
        public const float DefaultMissChance = 0.05f;
        public event OnDamageEventHandler OnDamageEvent;
        public float XpToGive { get; set; } = 8;
        public bool Immune { get; set; }
        public bool Delete { get; set; } = true;
        public float MissChance { get; set; } = DefaultMissChance;
        private readonly List<BaseBillboard> _damageLabels;
        private readonly List<Predicate<IEntity>> _ignoreList;
        private float _tintTimer;
        private Vector4 _targetTint;
        private float _attackedTimer;
        private bool _hasBeenAttacked;

        public DamageComponent(IEntity Parent) : base(Parent)
        {
            _damageLabels = new List<BaseBillboard>();
            _ignoreList = new List<Predicate<IEntity>>();
        }

        public override void Update()
        {
            _targetTint = _tintTimer > 0 ? new Vector4(2.0f, 0.1f, 0.1f, 1) : new Vector4(1, 1, 1, 1);

            if ((Parent.Model.Tint - _targetTint).LengthFast > 0.005f)
            {
                Parent.Model.Tint = Mathf.Lerp(Parent.Model.Tint, _targetTint, Time.DeltaTime * 12f);
            }

            _tintTimer -= Time.IndependantDeltaTime;
            _tintTimer = Math.Max(_tintTimer, 0);

            if (HasBeenAttacked)
            {
                _attackedTimer -= Time.IndependantDeltaTime;
                if (_attackedTimer < 0)
                {
                    _hasBeenAttacked = false;
                }
            }

            for (var i = _damageLabels.Count - 1; i > -1; i--)
            {
                if (_damageLabels[i].Disposed) _damageLabels.RemoveAt(i);
            }
        }

        public void Damage(float Amount, IEntity Damager, out float Exp, bool PlaySound, bool PushBack)
        {
            Exp = 0;
            
            Amount *= (1.0f / Parent.AttackResistance);
            if (Parent.IsDead || _ignoreList.Any(I => I.Invoke(Damager))) return;
            

            var shouldMiss = Parent is LocalPlayer && Utils.Rng.NextFloat() < MissChance;
            _attackedTimer = 6;
            _hasBeenAttacked = true;
            var isImmune = Immune | (Parent.IsStuck && !Parent.Model.Pause);

            if (!Parent.IsStatic && PlaySound && (GameManager.Player.Position - Parent.Position).LengthSquared < 80*80 && Amount >= 1f)
            {
                var asHuman = Damager as Humanoid;
                var baseDamage = Damager != null ? asHuman?.BaseDamageEquation 
                    ?? (Damager.SearchComponent<BasicAIComponent>() != null ? Damager.AttackDamage : Amount) : Amount / 3f;
                var color = Color.White;
                var dmgDiff = Amount / baseDamage;
                if (dmgDiff > 1.85f) color = Color.Gold;
                if (dmgDiff > 2.25f) color = Color.Red;
                if (isImmune || shouldMiss) color = Color.White;
                var maxSize = float.MaxValue;
                if (Parent is LocalPlayer)
                {
                    color = Color.Red;
                }
                var font = FontCache.Get(AssetManager.BoldFamily, Math.Min(12 + 6 * dmgDiff, maxSize), FontStyle.Bold);
                var dmgString = ((int) Amount).ToString();
                var missString = isImmune ? "IMMUNE" : "MISS";
                var dmgLabel = new TextBillboard(1.8f, !isImmune && !shouldMiss ? dmgString : missString, color,
                    font, () => Parent.Position)
                {
                    Vanish = true,
                    VanishSpeed = 4,
                };
                _damageLabels.Add(dmgLabel);
            }
            Exp = 0;

            if (PlaySound)
            {
                SoundPlayer.PlaySoundWithVariation(!shouldMiss ? SoundType.HitSound : SoundType.SlashSound, Parent.Position, 1f, 80f);
            }

            if (shouldMiss || isImmune) return;
            _tintTimer = 0.25f;
            Parent.Health = Math.Max(Parent.Health - Amount, 0);
            if (Damager != null && Damager != Parent && PushBack 
                && Parent.Size.LengthFast < Damager.Size.LengthFast)
            {
                var direction = -(Damager.Position - Parent.Position).Normalized();
                var factor = 0.5f;
                var averageSize = (Parent.Size.X + Parent.Size.Z) * .5f;
                if (Parent is LocalPlayer) factor = 0.0f;
                Parent.Physics.Translate(direction * factor * averageSize);
            }

            if (Parent.Health <= 0 && !Parent.IsDead)
            {
                Parent.IsDead = true;
                Parent.Physics.CollidesWithEntities = false;
                Exp = XpToGive;
                if(Damager is LocalPlayer)
                {
                    Parent.ShowText($"+{(int)Math.Ceiling(Exp)} XP", Color.Violet, 20);
                }
                RoutineManager.StartRoutine(this.DisposeCoroutine);
                
            }
            if (OnDamageEvent != null && Math.Abs(Amount) > 0.005f)
                OnDamageEvent.Invoke(new DamageEventArgs(Parent, Damager, Amount, Exp));
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
                    currentTime += Time.IndependantDeltaTime * 2.0f;
                    animable.DisposeTime = currentTime;
                    yield return null;
                }
            }
            else
            {
                while (currentTime < 4)
                {
                    currentTime += Time.IndependantDeltaTime * 1f;
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

        public BaseBillboard[] Labels => _damageLabels.ToArray();

        /// <summary>
        /// Returns a bool representing if the Entity has been attacked in the last six seconds.
        /// </summary>
        public bool HasBeenAttacked => _hasBeenAttacked;
    }

    public class DamageEventArgs : EventArgs
    {
        public IEntity Victim { get; }
        public IEntity Damager { get; }
        public float Amount { get; }
        public float Experience { get; }

        public DamageEventArgs(IEntity Victim, IEntity Damager, float Amount, float Experience)
        {
            this.Victim = Victim;
            this.Damager = Damager;
            this.Amount = Amount;
            this.Experience = Experience;
        }
    }
}