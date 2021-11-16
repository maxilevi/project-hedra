/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:13 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Core;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem
{
    public abstract class PlayerSingleAnimationSkill : SingleAnimationSkill<IPlayer>
    {
    }

    public abstract class SingleAnimationSkill<T> : CappedSkill<T>
        where T : class, IObjectWithAnimation, ISkillUser, IObjectWithMovement, IObjectWithWeapon, IObjectWithLifeCycle
    {
        private bool _executedEnd;
        private bool _executedMid;
        private bool _executedStart;
        private float _frameCounter;
        private bool _shouldEnd;

        protected SingleAnimationSkill()
        {
            if (SkillAnimation != SkillAnimation)
                throw new ArgumentOutOfRangeException("SkillAnimation needs to be a singleton.");
            SkillAnimation.Loop = false;
            SkillAnimation.OnAnimationStart += Sender =>
            {
                if (!_executedStart)
                    OnAnimationStart();
                _executedStart = true;
            };
            SkillAnimation.OnAnimationMid += Sender =>
            {
                if (!_executedMid)
                    OnAnimationMid();
                _executedMid = true;
            };
            SkillAnimation.OnAnimationEnd += Sender =>
            {
                if (!_executedEnd)
                    OnAnimationEnd();
                _executedEnd = true;
                _shouldEnd = true;
            };
        }

        protected abstract Animation SkillAnimation { get; }
        protected virtual float AnimationSpeed { get; } = 1;
        protected virtual bool EquipWeapons => true;
        protected virtual bool CanMoveWhileCasting => true;
        protected virtual bool ShouldCancel => User.AnimationBlending != SkillAnimation;
        public override float IsAffectingModifier => Casting ? 1 : 0;

        private bool ShouldEnd => User.IsDead || User.IsKnocked || _shouldEnd || ShouldCancel;

        protected override void DoUse()
        {
            Casting = true;
            if (EquipWeapons)
            {
                User.IsAttacking = true;
            }
            else
            {
                User.IsAttacking = false;
                User.WasAttacking = false;
                User.InAttackStance = false;
            }

            if (!CanMoveWhileCasting) User.CaptureMovement = false;
            User.Orientate();
            _shouldEnd = false;
            _executedStart = false;
            _executedMid = false;
            _executedEnd = false;
            SkillAnimation.Speed = AnimationSpeed;
            User.BlendAnimation(SkillAnimation);
            OnEnable();
        }

        private void Disable()
        {
            Casting = false;
            User.CaptureMovement = true;
            User.ResetModel();
            OnDisable();
        }

        public override void Update()
        {
            var t = GetType();
            base.Update();
            if (!Casting) return;
            if (ShouldEnd) Disable();

            if (_frameCounter >= .25f)
            {
                OnQuarterSecond();
                _frameCounter = 0;
            }

            _frameCounter += Time.DeltaTime;
            OnExecution();
        }

        protected void Cancel()
        {
            _shouldEnd = true;
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void OnEnable()
        {
        }

        protected virtual void OnAnimationMid()
        {
        }

        protected virtual void OnQuarterSecond()
        {
        }

        protected virtual void OnAnimationStart()
        {
        }

        protected virtual void OnAnimationEnd()
        {
        }

        protected virtual void OnExecution()
        {
        }
    }
}