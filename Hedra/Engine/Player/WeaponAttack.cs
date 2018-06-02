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
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ItemSystem.WeaponSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Player.ToolbarSystem;
using OpenTK;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Rendering;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of WeaponAttack.
	/// </summary>
	public class WeaponAttack : BaseSkill
	{
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

		public bool DisableWeapon {get; set;}
	    private bool _isPressing;
        private AttackType _type;
	    private float _maxCooldown;

        public WeaponAttack() : base() {
			base.ManaCost = 0f;
			base.Level = 1;
            _maxCooldown = 0.25f;
		    base.TexId = Default;
        }
		
		public void SetType(Weapon Weapon, AttackType Type)
		{
		    this._type = Type;
		    var flags = BindingFlags.Static | BindingFlags.NonPublic;
		    var fieldInfo1 = this.GetType().GetField($"{Weapon.GetType().Name}1", flags);
		    var fieldInfo2 = this.GetType().GetField($"{Weapon.GetType().Name}2", flags);
		    base.TexId = (uint) ((Type == AttackType.Primary ? fieldInfo1?.GetValue(null) : fieldInfo2?.GetValue(null)) ?? (uint) Default);
		    _maxCooldown = Type == AttackType.Primary ? Weapon.PrimaryAttackCooldown : Weapon.SecondaryAttackCooldown;
		}
		
		public override bool MeetsRequirements(Toolbar Bar, int CastingAbilityCount)
		{
			if(DisableWeapon) return false;			
			 return base.MeetsRequirements(Bar, CastingAbilityCount) && !Player.IsAttacking && !Player.IsEating && Player.CanInteract;
		}
		
		public override void KeyUp()
		{
			_isPressing = false;
		}
		
		public override void Use(){
			
			_isPressing = true;
            if (_type == AttackType.Primary) Player.Model.LeftWeapon.Attack1(Player);
		    if (_type == AttackType.Secondary) Player.Model.LeftWeapon.Attack2(Player);	
		}
		
		public override void Update(){
			if(DisableWeapon) return;
		    this.MaxCooldown = _maxCooldown / Player.AttackSpeed;
			if(_isPressing && Cooldown < 0){
				Player.Model.LeftWeapon.Attack1(Player);
				Player.Model.LeftWeapon.ContinousAttack = true;
			}
			else{
				Player.Model.LeftWeapon.ContinousAttack = false;
			}
		}
		
		public override string Description => string.Empty;
	}

    public enum AttackType
    {
        Primary,
        Secondary
    }
}
