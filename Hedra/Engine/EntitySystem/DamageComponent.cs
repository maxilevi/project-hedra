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
using Hedra.Engine.AISystem;
using Hedra.Engine.Player;
using Hedra.Engine.Sound;

namespace Hedra.Engine.EntitySystem
{
    /// <summary>
    /// Description of DamageComponent.
    /// </summary>
    public delegate void OnDamageEventHandler(DamageEventArgs Args);

    public class DamageComponent : EntityComponent
    {
        public float XpToGive = 8;
        public bool Immune = false;
        public bool Delete = true;
        public readonly List<Billboard> DamageLabels;
        public event OnDamageEventHandler OnDamageEvent;

        public DamageComponent(IEntity Parent) : base(Parent)
        {
            DamageLabels = new List<Billboard>();
        }

        private float _tintTimer;
        private Vector4 _targetTint;

        public override void Update()
        {
            _targetTint = _tintTimer > 0 ? new Vector4(2.0f, 0.1f, 0.1f, 1) : new Vector4(1, 1, 1, 1);

            if ((Parent.Model.Tint - _targetTint).LengthFast > 0.005f)
            {
                Parent.Model.Tint = Mathf.Lerp(Parent.Model.Tint, _targetTint, (float) Time.DeltaTime * 12f);
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

            for (int i = DamageLabels.Count - 1; i > -1; i--)
            {
                if (DamageLabels[i].Disposed) DamageLabels.RemoveAt(i);
            }
        }

        public void Damage(float Amount, IEntity Damager, out float Exp, bool PlaySound)
        {
            if (this.Parent.AttackResistance > 0)
            {
                Amount /= this.Parent.AttackResistance;
            }

            if (Parent.IsDead)
            {
                Exp = 0;
                return;
            }

            bool shouldMiss = Parent is LocalPlayer && Utils.Rng.Next(1, 18) == 1;
            _attackedTimer = 6;
            _hasBeenAttacked = true;

            if (!Parent.IsStatic && PlaySound && (GameManager.Player.Position - Parent.Position).LengthSquared < 80*80 && Amount >= 1f)
            {
                var baseDamage = Damager != null ? (Damager as Humanoid)?.BaseDamageEquation 
                    ?? (Damager.SearchComponent<BasicAIComponent>() != null ? Damager.AttackDamage * .3f : Amount * .3f) : Amount / 3f;
                Color color = Color.White;
                float dmgDiff = Amount / baseDamage;
                if (dmgDiff > 1.85f) color = Color.Gold;
                if (dmgDiff > 2.25f) color = Color.Red;
                if (!Immune && !shouldMiss) color = Color.White;
                var font = FontCache.Get(AssetManager.BoldFamily, 12 + 32 * (Amount / Parent.MaxHealth), FontStyle.Bold);
                var dmgString = ((int) Amount).ToString();
                var missString = Immune ? "IMMUNE" : "MISS";
                var dmgLabel = new Billboard(1.8f, !Immune && !shouldMiss ? dmgString : missString, color,
                    font, Parent.Position)
                {
                    Vanish = true,
                    Speed = 4,
                    FollowFunc = () => Parent.Position
                };
                DamageLabels.Add(dmgLabel);
            }
            Exp = 0;

            if (PlaySound)
            {
                SoundManager.PlaySoundWithVariation(!shouldMiss ? SoundType.HitSound : SoundType.SlashSound, Parent.Position, 1f, 80f);
            }

            if (shouldMiss || Immune) return;
            _tintTimer = 0.25f;
            Parent.Health = Math.Max(Parent.Health - Amount, 0);
            if (Damager != null && Damager != Parent)
            {
                Vector3 direction = -(Damager.Position - Parent.Position).Normalized();
                var factor = 0.5f;
                var averageSize = (Parent.Model.BaseBroadphaseBox.Size.X + Parent.Model.BaseBroadphaseBox.Size.Z) * .5f;
                if (Parent is LocalPlayer) factor = 0.0f;
                Parent.Physics.Translate(direction * factor * averageSize);
            }

            if (Parent.Health == 0 && !Parent.IsDead)
            {
                Exp = (int) Math.Ceiling(XpToGive);
                if (!Parent.IsStatic && Damager is IPlayer)
                {
                    var label = new Billboard(4.0f, "+" + Exp + " XP", Color.Violet,
                        FontCache.Get(AssetManager.BoldFamily, 48, FontStyle.Bold),
                        Parent.Model.Position)
                    {
                        Size = .4f,
                        Vanish = true
                    };
                }

                Parent.IsDead = true;
                var dropComponent = Parent.SearchComponent<DropComponent>();
                dropComponent?.Drop();
                if (Delete)
                {
                    Parent.Physics.HasCollision = false;
                    CoroutineManager.StartCoroutine(this.DisposeCoroutine);
                }
            }
            if (OnDamageEvent != null && Amount != 0)
                OnDamageEvent.Invoke(new DamageEventArgs(Parent, Damager, Amount, Exp));
        }


        public IEnumerator DisposeCoroutine()
        {
            float currentTime = 0;

            if (Parent.Model is IDisposeAnimation animable)
            {
                SoundManager.PlaySound(SoundType.GlassBreak, Parent.Position);
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

            animable = null;

            if (!(Parent is LocalPlayer))
                Parent.Dispose();
        }

        private float _attackedTimer;
        private bool _hasBeenAttacked;

        /// <summary>
        /// Returns a bool representing if the Entity has been attacked in the last six seconds.
        /// </summary>
        public bool HasBeenAttacked => _hasBeenAttacked;
    }

    public class DamageEventArgs : EventArgs
    {
        public IEntity Victim;
        public IEntity Damager;
        public float Amount;
        public float Experience;

        public DamageEventArgs(IEntity Victim, IEntity Damager, float Amount, float Experience)
        {
            this.Victim = Victim;
            this.Damager = Damager;
            this.Amount = Amount;
            this.Experience = Experience;
        }
    }
}