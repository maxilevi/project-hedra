/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 22/07/2016
 * Time: 08:59 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
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
        private ShiverAnimation _shiverAnimation;
        private bool _continousAttack;
        public float Charge { get; private set; }
        private float _chargeTime;
        private bool _isCharging;
        private AttackType _type;
        private uint _textureId = Default;

        public WeaponAttack()
        {
            _shiverAnimation = new ShiverAnimation();
            base.ManaCost = 0f;
            base.Level = 1;
            this.MaxCooldown = .5f;
        }
        
        public void SetType(Weapon Weapon, AttackType Type)
        {
            _type = Type;
            _textureId = Type == AttackType.Primary
                ? Weapon?.PrimaryAttackIcon ?? WeaponIcons.DefaultAttack
                : Weapon?.SecondaryAttackIcon ?? WeaponIcons.DefaultAttack;
        }
        
        public override bool MeetsRequirements()
        {
            if(DisableWeapon) return false;            
             return base.MeetsRequirements() && !Player.IsAttacking && !Player.IsEating && Player.CanInteract;
        }
        
        public override void KeyUp()
        {
            if (_type == AttackType.Primary)
            {
                _continousAttack = false;
            }
            if (_type == AttackType.Secondary && _isCharging)
            {
                var charge = _chargeTime / (BaseChargeTime + ExtraChargeTime);
                Player.LeftWeapon.Attack2(Player, new AttackOptions
                {
                    Charge = charge,
                    DamageModifier = AttackOptions.Default.DamageModifier * charge + .15f
                });
                IsCharging = false;
            }
        }
        
        public override void Use()
        {
            if (_type == AttackType.Primary)
            {
                _continousAttack = true;
            }
            if (_type == AttackType.Secondary)
            {
                IsCharging = true;
                SoundPlayer.PlaySoundWhile(SoundType.PreparingAttack, () => IsCharging, () => 1, () => Charge);
            }
        }
        
        public override void Update()
        {
            if(DisableWeapon) return;
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
            Tint = IsCharging ? new Vector3(1,0,0) * Charge + Vector3.One * .65f : NormalTint;
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
