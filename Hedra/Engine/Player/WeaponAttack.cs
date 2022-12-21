/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 22/07/2016
 * Time: 08:59 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.SkillSystem;
using Hedra.Rendering;
using Hedra.Sound;
using Hedra.WeaponSystem;

namespace Hedra.Engine.Player
{
    /// <summary>
    ///     Description of WeaponAttack.
    /// </summary>
    public class WeaponAttack : BaseSkill<IPlayer>
    {
        private const float BaseChargeTime = 2.0f;
        private const float ExtraChargeTime = 2.5f;
        private static readonly uint Default = Graphics2D.LoadFromAssets("Assets/Skills/HolderSkill.png");
        private readonly ShiverAnimation _shiverAnimation;
        private float _chargeTime;
        private bool _continousAttack;
        private bool _isCharging;
        private uint _textureId = Default;
        private AttackType _type;

        public WeaponAttack()
        {
            _shiverAnimation = new ShiverAnimation();
            Level = 1;
        }

        public bool DisableWeapon { get; set; }
        public float Charge { get; private set; }

        public bool IsCharging
        {
            get => _isCharging;
            set
            {
                _isCharging = value;
                _chargeTime = value ? _chargeTime : 0;
                Charge = value ? Charge : 0;
                User.LeftWeapon.Charging = value;
                if (value) _shiverAnimation.Play(this);
                else _shiverAnimation.Stop();
            }
        }

        protected override bool ShouldDisable => !CanUse();
        private bool ShouldCharge => AttackType.Secondary == _type || User is { LeftWeapon.IsChargeable: true };
        public override uint IconId => _textureId;

        protected override bool HasCooldown => AttackType.Secondary == _type
            ? User.LeftWeapon.SecondaryAttackHasCooldown
            : User.LeftWeapon.PrimaryAttackHasCooldown;

        public override float ManaCost => 0;

        public override float MaxCooldown => AttackType.Secondary == _type
            ? User.LeftWeapon.SecondaryAttackCooldown
            : User.LeftWeapon.PrimaryAttackCooldown;

        public override string Description => string.Empty;
        public override string DisplayName => string.Empty;

        public void SetType(Weapon Weapon, AttackType Type)
        {
            _type = Type;
            _textureId = Type == AttackType.Primary
                ? Weapon?.PrimaryAttackIcon ?? WeaponIcons.DefaultAttack
                : Weapon?.SecondaryAttackIcon ?? WeaponIcons.DefaultAttack;
        }

        public override void KeyUp()
        {
            if (!ShouldCharge)
            {
                _continousAttack = false;
            }
            else if (_isCharging)
            {
                var charge = _chargeTime / (BaseChargeTime + ExtraChargeTime);
                var options = new AttackOptions
                {
                    Charge = Math.Min(charge * (1 + User.Sorcery / 100f), 1f),
                    DamageModifier = AttackOptions.Default.DamageModifier * charge + .15f
                };
                if (_type == AttackType.Primary)
                    User.LeftWeapon.Attack1(User, options);
                else
                    User.LeftWeapon.Attack2(User, options);
                IsCharging = false;
            }
        }

        protected override void DoUse()
        {
            if (ShouldCharge)
            {
                IsCharging = true;
                SoundPlayer.PlaySoundWhile(SoundType.PreparingAttack, () => IsCharging, () => 1, () => Charge);
            }
            else
            {
                _continousAttack = true;
            }
        }

        public override void Update()
        {
            if (DisableWeapon) return;
            if (User.IsDead) _continousAttack = false;
            if (_continousAttack) User.LeftWeapon.Attack1(User);
            if (IsCharging)
            {
                User.LeftWeapon.InAttackStance = true;
                User.Movement.Orientate();
                _chargeTime = Math.Min(_chargeTime + Time.DeltaTime, BaseChargeTime + ExtraChargeTime);
                Charge = _chargeTime / (BaseChargeTime + ExtraChargeTime);
                _shiverAnimation.Intensity = Charge;
                _shiverAnimation.Update();
                User.LeftWeapon.ChargingIntensity = Charge;
            }

            User.LeftWeapon.IsCharging = IsCharging;
            Tint = IsCharging ? new Vector3(1, 0, 0) * Charge + Vector3.One * .65f : Vector3.One;
        }

        private bool CanUse()
        {
            return !DisableWeapon && !User.IsAttacking && !User.IsEating && User.CanInteract &&
                   (User.LeftWeapon.PrimaryAttackEnabled && _type == AttackType.Primary ||
                    User.LeftWeapon.SecondaryAttackEnabled && _type == AttackType.Secondary);
        }
    }

    public enum AttackType
    {
        Primary,
        Secondary
    }
}