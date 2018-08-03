/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 22/07/2016
 * Time: 08:59 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Reflection;
using Hedra.Engine.ItemSystem.WeaponSystem;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Player.ToolbarSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Sound;
using OpenTK;

namespace Hedra.Engine.Player
{
    /// <summary>
    /// Description of WeaponAttack.
    /// </summary>
    [Obfuscation(Exclude = false, Feature = "-rename")]
    public class WeaponAttack : BaseSkill
	{
		private const float ChargeTime = 1;
	    private static readonly uint Default = Graphics2D.LoadFromAssets("HolderSkill.png");
        private static readonly uint Sword1 = Graphics2D.LoadFromAssets("Slash.png");
		private static readonly uint Sword2 = Graphics2D.LoadFromAssets("Lunge.png");
		private static readonly uint Knife1 = Graphics2D.LoadFromAssets("SlashKnife.png");
		private static readonly uint Knife2 = Graphics2D.LoadFromAssets("LungeKnife.png");
		private static readonly uint Bow1 = Graphics2D.LoadFromAssets("Shoot.png");
		private static readonly uint Bow2 = Graphics2D.LoadFromAssets("TripleShot.png");
		private static readonly uint Axe1 = Graphics2D.LoadFromAssets("SwingAxeIcon.png");
		private static readonly uint Axe2 = Graphics2D.LoadFromAssets("SmashAxeIcon.png");
		private static readonly uint Hammer1 = Graphics2D.LoadFromAssets("SwingHammerIcon.png");
		private static readonly uint Hammer2 = Graphics2D.LoadFromAssets("SmashHammerIcon.png");
		private static readonly uint DoubleBlades1 = Graphics2D.LoadFromAssets("BladesAttack1.png");
		private static readonly uint DoubleBlades2 = Graphics2D.LoadFromAssets("BladesAttack2.png");
		private static readonly uint Katar1 = Graphics2D.LoadFromAssets("KatarAttack1.png");
		private static readonly uint Katar2 = Graphics2D.LoadFromAssets("KatarAttack2.png");
		private static readonly uint Claw1 = Graphics2D.LoadFromAssets("ClawAttack1.png");
		private static readonly uint Claw2 = Graphics2D.LoadFromAssets("ClawAttack2.png");

		public bool DisableWeapon { get; set; }
	    private bool _continousAttack;
		private float _charge;
		private bool _isCharging;
        private AttackType _type;

        public WeaponAttack()
        {
			base.ManaCost = 0f;
			base.Level = 1;
		    base.TextureId = Default;
            this.MaxCooldown = .5f;
        }
		
		public void SetType(Weapon Weapon, AttackType Type)
		{
            this._type = Type;
		    var flags = BindingFlags.Static | BindingFlags.NonPublic;
		    var fieldInfo1 = this.GetType().GetField($"{Weapon.GetType().Name}1", flags);
		    var fieldInfo2 = this.GetType().GetField($"{Weapon.GetType().Name}2", flags);
		    base.TextureId = (uint) ((Type == AttackType.Primary ? fieldInfo1?.GetValue(null) : fieldInfo2?.GetValue(null)) ?? (uint) Default);
		}
		
		public override bool MeetsRequirements(Toolbar Bar, int CastingAbilityCount)
		{
			if(DisableWeapon) return false;			
			 return base.MeetsRequirements(Bar, CastingAbilityCount) && !Player.IsAttacking && !Player.IsEating && Player.CanInteract;
		}
		
		public override void KeyUp()
		{
		    if (_type == AttackType.Primary)
		    {
		        _continousAttack = false;
		    }
		    if (_type == AttackType.Secondary && _isCharging)
			{
				Player.Model.LeftWeapon.Attack2(Player, new AttackOptions
				{
				    Charge = _charge
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
	            SoundManager.PlaySoundWhile(SoundType.PreparingAttack, () => IsCharging);
            }
		}
		
		public override void Update()
        {
			if(DisableWeapon) return;
			if(_continousAttack)
			{
                Player.Model.LeftWeapon.Attack1(Player);
			}
	        if (IsCharging)
	        {
                Player.Movement.Orientate();
		        _charge = Math.Min(_charge + Time.DeltaTime, ChargeTime);
	        }
		}

		public bool IsCharging
		{
			get => _isCharging;
			set
			{
				_isCharging = value;
				_charge = value ? _charge : 0;
				Tint = value ? new Vector3(2, .75f, .75f) : NormalTint;
			}
		}

		protected override bool UseTextureIdCache => false;
	    protected override bool HasCooldown => false;
		public override string Description => string.Empty;
	}

    public enum AttackType
    {
        Primary,
        Secondary
    }
}
