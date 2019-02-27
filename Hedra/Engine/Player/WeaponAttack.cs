/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 22/07/2016
 * Time: 08:59 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.SkillSystem;
using Hedra.Sound;
using Hedra.WeaponSystem;
using OpenTK;

namespace Hedra.Engine.Player
{
    /// <summary>
    /// Description of WeaponAttack.
    /// </summary>
    public class WeaponAttack : BaseSkill
    {
        private const float BaseChargeTime = 2.0f;
        private const float ExtraChargeTime = 2.5f;
        private static readonly uint Default = Graphics2D.LoadFromAssets("Assets/Skills/HolderSkill.png");

        public bool DisableWeapon { get; set; }
        private readonly ShiverAnimation _shiverAnimation;
        private bool _continousAttack;
        public float Charge { get; private set; }
        private float _chargeTime;
        private bool _isCharging;
        private AttackType _type;
        private uint _textureId = Default;

        public WeaponAttack()
        {
            _shiverAnimation = new ShiverAnimation();
            Level = 1;
        }
        
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
                    Charge = charge,
                    DamageModifier = AttackOptions.Default.DamageModifier * charge + .15f
                };
                if(_type == AttackType.Primary)
                    Player.LeftWeapon.Attack1(Player, options);
                else
                    Player.LeftWeapon.Attack2(Player, options);
                IsCharging = false;
            }
        }
        
        public override void Use()
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
            if(DisableWeapon) return;
            if (Player.IsDead) _continousAttack = false;
            if(_continousAttack)
            {
                Player.LeftWeapon.Attack1(Player);
            }
            if (IsCharging)
            {
                Player.LeftWeapon.InAttackStance = true;
                Player.Movement.Orientate();
                _chargeTime = Math.Min(_chargeTime + Time.DeltaTime, BaseChargeTime + ExtraChargeTime);
                Charge = _chargeTime / (BaseChargeTime + ExtraChargeTime);
                _shiverAnimation.Intensity = Charge;
                _shiverAnimation.Update();
                Player.LeftWeapon.ChargingIntensity = Charge;
            }
            Player.LeftWeapon.IsCharging = IsCharging;
            Tint = IsCharging ? new Vector3(1,0,0) * Charge + Vector3.One * .65f : Vector3.One;
        }

        public bool IsCharging
        {
            get => _isCharging;
            set
            {
                _isCharging = value;
                _chargeTime = value ? _chargeTime : 0;
                Charge = value ? Charge : 0;
                Player.LeftWeapon.Charging = value; 
                if(value) _shiverAnimation.Play(this);
                else _shiverAnimation.Stop();
            }
        }

        private bool CanUse()
        {
            return !DisableWeapon && (!Player.IsAttacking && !Player.IsEating && Player.CanInteract
                    && (Player.LeftWeapon.PrimaryAttackEnabled && _type == AttackType.Primary ||
                        Player.LeftWeapon.SecondaryAttackEnabled && _type == AttackType.Secondary));
        }
        
        protected override bool ShouldDisable => !CanUse();
        private bool ShouldCharge => AttackType.Secondary == _type || Player.LeftWeapon.IsChargeable;
        public override uint TextureId => _textureId;
        protected override bool HasCooldown => false;
        public override string Description => string.Empty;
        public override string DisplayName => string.Empty;
    }

    public enum AttackType
    {
        Primary,
        Secondary
    }
}
