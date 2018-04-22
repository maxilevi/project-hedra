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
using Hedra.Engine.ItemSystem;
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
        public float Armor = 1;
        public float XpToGive = 8;
        public bool Immune = false;
        public bool Delete = true;
        public List<Billboard> DamageLabels = new List<Billboard>();
        public event OnDamageEventHandler OnDamageEvent;

        public DamageComponent(Entity Parent) : base(Parent){}

        private float _tintTimer = 0;
        private Vector4 _targetTint;
        private int _consecutiveHits = 0;
        private readonly Timer _hitsTimer = new Timer(3);

        public override void Update()
        {
            if (_hitsTimer.Tick())
                _consecutiveHits = 0;

            _targetTint = _tintTimer > 0 ? new Vector4(2.0f, 0.1f, 0.1f, 1) : new Vector4(1, 1, 1, 1);

            Parent.Model.Tint = Mathf.Lerp(Parent.Model.Tint, _targetTint, (float) Time.deltaTime * 12f);

            _tintTimer -= Time.FrameTimeSeconds;
            _tintTimer = Math.Max(_tintTimer, 0);

            if (HasBeenAttacked)
            {
                _attackedTimer -= Time.FrameTimeSeconds;
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

        public void Damage(float Amount, Entity Damager, out float Exp, bool PlaySound)
        {
            if (Parent.IsDead)
            {
                Exp = 0;
                return;
            }

            bool shouldMiss = Parent is LocalPlayer && Utils.Rng.Next(1, 18) == 1;
            var damager = Damager as Humanoid;
            //if (damager?.MainWeapon?.Type == ItemType.ThrowableDagger)
            //    Amount *= 1f + (float) Math.Min(_consecutiveHits * .2, 1.2f);

            _consecutiveHits++;
            _hitsTimer.Reset();

            _attackedTimer = 6;
            _hasBeenAttacked = true;

            if (!Parent.IsStatic && PlaySound && (LocalPlayer.Instance.Position - Parent.Position).LengthSquared < 80*80 && Amount >= 1f)
            {
                var baseDamage = Damager != null ? (Damager as Humanoid)?.BaseDamageEquation 
                    ?? (Damager.SearchComponent<AIComponent>() != null ? Damager.AttackDamage * .3f : Amount * .3f) : Amount / 3f;
                Color color = Color.White;
                float dmgDiff = Amount / baseDamage;
                if (dmgDiff > 1.85f) color = Color.Gold;
                if (dmgDiff > 2.25f) color = Color.Red;
                Billboard dmgLabel;
                if (!Immune && !shouldMiss)
                    dmgLabel = new Billboard(1.8f, ((int) Amount).ToString(), color,
                        FontCache.Get(AssetManager.Fonts.Families[0], 12 + 32 * (Amount / Parent.MaxHealth),
                            FontStyle.Bold), Parent.Model.Position);
                else
                    dmgLabel = new Billboard(1.8f, Immune ? "IMMUNE" : "MISS", Color.White,
                        FontCache.Get(AssetManager.Fonts.Families[0], 12 + 32 * (Amount / Parent.MaxHealth),
                            FontStyle.Bold), Parent.Model.Position);
                dmgLabel.Vanish = true;
                dmgLabel.Speed = 4;
                dmgLabel.FollowFunc = () => Parent.Position;
                DamageLabels.Add(dmgLabel);
            }
            Exp = 0;
            if (shouldMiss || Immune) return;
            _tintTimer = 0.25f;
            Parent.Health = Math.Max(Parent.Health - Amount, 0);
            if (Damager != null && Damager != Parent)
            {
                Vector3 direction = -(Damager.Position - Parent.Position).Normalized();
                for (int i = 0; i < 10; i++)
                    Parent.Physics.Move(direction * 1.5f);
            }

            if (PlaySound)
                SoundManager.PlaySoundWithVariation(SoundType.HitSound, Parent.Position, 1f, 80f);

            if (Parent.Health == 0 && !Parent.IsDead)
            {
                if (!Parent.IsStatic && Damager is LocalPlayer)
                {
                    Exp = (int) XpToGive;
                    var label = new Billboard(4.0f, "+" + Exp + " XP", Color.Violet,
                        FontCache.Get(AssetManager.Fonts.Families[0], 48, FontStyle.Bold),
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

            var animable = (Parent.Model as IDisposeAnimation);
            if (animable != null)
            {
                SoundManager.PlaySound(SoundType.GlassBreak, Parent.Position);
                animable.DisposeAnimation();

                while (currentTime < 4)
                {
                    currentTime += Time.FrameTimeSeconds * 2.0f;
                    animable.DisposeTime = currentTime;
                    yield return null;
                }
            }
            else
            {
                while (currentTime < 4)
                {
                    currentTime += Time.FrameTimeSeconds * 1f;
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
        public Entity Victim;
        public Entity Damager;
        public float Amount;
        public float Experience;

        public DamageEventArgs(Entity Victim, Entity Damager, float Amount, float Experience)
        {
            this.Victim = Victim;
            this.Damager = Damager;
            this.Amount = Amount;
            this.Experience = Experience;
        }
    }
}