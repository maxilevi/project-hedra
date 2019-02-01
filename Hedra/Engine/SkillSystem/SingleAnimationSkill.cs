/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:13 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.Generation;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Rendering.Particles;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.SkillSystem
{
    /// <summary>
    /// Description of WeaponThrow.
    /// </summary>
    public abstract class SingleAnimationSkill : CappedSkill
    {
        protected abstract Animation SkillAnimation { get; }
        protected virtual float AnimationSpeed { get; }
        protected virtual bool EquipWeapons => true;
        private float _frameCounter;
        private bool _shouldEnd;
        private bool _executedStart;
        private bool _executedMid;
        private bool _executedEnd;

        protected SingleAnimationSkill() 
        {
            if(SkillAnimation != SkillAnimation) throw new ArgumentOutOfRangeException($"SkillAnimation needs to be a singleton.");
            SkillAnimation.Loop = false;
            SkillAnimation.Speed = AnimationSpeed;
            SkillAnimation.OnAnimationStart += Sender =>
            {
                if(!_executedStart)
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
        
        public override void Use()
        {
            Casting = true;
            if (EquipWeapons)
            {
                Player.IsAttacking = true;
            }
            else
            {
                Player.IsAttacking = false;
                Player.WasAttacking = false;
                Player.LeftWeapon.InAttackStance = false;
            }
            Player.Movement.Orientate();
            _shouldEnd = false;
            _executedStart = false;
            _executedMid = false;
            _executedEnd = false;
            Player.Model.BlendAnimation(SkillAnimation);
        }

        private void Disable()
        {
            Casting = false;
            Player.Model.Reset();
            OnDisable();
        }
        
        public override void Update()
        {
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

        private bool ShouldEnd => Player.IsDead || Player.IsKnocked || _shouldEnd || Player.Model.AnimationBlending != SkillAnimation;

        protected virtual void OnDisable()
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

        protected abstract void OnExecution();
    }
}