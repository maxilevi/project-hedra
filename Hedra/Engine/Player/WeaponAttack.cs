/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 22/07/2016
 * Time: 08:59 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using OpenTK;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Rendering;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of WeaponAttack.
	/// </summary>
	public class WeaponAttack : Skill
	{
		private static readonly uint SlashIcon = Graphics2D.LoadFromAssets("Slash.png");
		private static readonly uint LungeIcon = Graphics2D.LoadFromAssets("Lunge.png");
		private static readonly uint SlashKnifeIcon = Graphics2D.LoadFromAssets("SlashKnife.png");
		private static readonly uint LungeKnifeIcon = Graphics2D.LoadFromAssets("LungeKnife.png");
		private static readonly uint ShootIcon = Graphics2D.LoadFromAssets("Shoot.png");
		private static readonly uint TripleShotIcon = Graphics2D.LoadFromAssets("TripleShot.png");
		private static readonly uint ThrowIcon = Graphics2D.LoadFromAssets("Knife.png");
		private static readonly uint ThrowSpecialIcon = Graphics2D.LoadFromAssets("DoubleKnife.png");
		private static readonly uint SwingAxeIcon = Graphics2D.LoadFromAssets("SwingAxeIcon.png");
		private static readonly uint SmashAxeIcon = Graphics2D.LoadFromAssets("SmashAxeIcon.png");
		private static readonly uint SwingHammerIcon = Graphics2D.LoadFromAssets("SwingHammerIcon.png");
		private static readonly uint SmashHammerIcon = Graphics2D.LoadFromAssets("SmashHammerIcon.png");
		private static readonly uint Blade1Icon = Graphics2D.LoadFromAssets("BladesAttack1.png");
		private static readonly uint Blade2Icon = Graphics2D.LoadFromAssets("BladesAttack2.png");
		private static readonly uint Katar1Icon = Graphics2D.LoadFromAssets("KatarAttack1.png");
		private static readonly uint Katar2Icon = Graphics2D.LoadFromAssets("KatarAttack2.png");
		private static readonly uint Claw1Icon = Graphics2D.LoadFromAssets("ClawAttack1.png");
		private static readonly uint Claw2Icon = Graphics2D.LoadFromAssets("ClawAttack2.png");
		private AttackType Type;
		public bool DisableWeapon {get; set;}
		
		public WeaponAttack(AttackType Type, Vector2 Position, Vector2 Scale, Panel InPanel, LocalPlayer Player) : base(Position, Scale, InPanel, Player) {
			this.Type = Type;
			this.SetType(Type);
			base.ManaCost = 0f;
			base.Level = 1;
		}
		
		public void SetType(AttackType Type){
			this.Type = Type;	
			
			if(Type == AttackType.KATAR1)
				base.TexId = Katar1Icon;
			
			if (Type == AttackType.KATAR2)
				base.TexId = Katar2Icon;
			
			if(Type == AttackType.BLADE1)
				base.TexId = Blade1Icon;
			
			if (Type == AttackType.BLADE2)
				base.TexId = Blade2Icon;
			
			if(Type == AttackType.CLAW1)
				base.TexId = Claw1Icon;
			
			if (Type == AttackType.CLAW2)
				base.TexId = Claw2Icon;
			
			
			if(Type == AttackType.SLASH)
				base.TexId = SlashIcon;
			
			if (Type == AttackType.LUNGE)
				base.TexId = LungeIcon;
			
			if(Type == AttackType.KNIFE_SLASH)
				base.TexId = SlashKnifeIcon;
			
			if (Type == AttackType.KNIFE_LUNGE)
				base.TexId = LungeKnifeIcon;
			
			if (Type == AttackType.SHOOT)
				base.TexId = ShootIcon;
			
			if(Type == AttackType.TRIPLESHOT)
				base.TexId = TripleShotIcon;
			
			if (Type == AttackType.THROW)
				base.TexId = ThrowIcon;
			
			if(Type == AttackType.THROW_SPECIAL)
				base.TexId = ThrowSpecialIcon;
			
			if (Type == AttackType.SWING){
				if(Player.Inventory.MainWeapon.EquipmentType == EquipmentType.Hammer.ToString())
					base.TexId = SwingHammerIcon;
				else
					base.TexId = SwingAxeIcon;
			}
			
			if(Type == AttackType.SMASH){
				if(Player.Inventory.MainWeapon.EquipmentType == EquipmentType.Hammer.ToString())
					base.TexId = SmashHammerIcon;
				else
					base.TexId = SmashAxeIcon;
			}
			
			if(Type == AttackType.BLADE1)
				base.MaxCooldown = 0.3f - Player.AttackSpeed / 50f;
			
			if(Type == AttackType.CLAW1)
				base.MaxCooldown = 0.1f - Player.AttackSpeed / 50f;
			
			if(Type == AttackType.KATAR1)
				base.MaxCooldown = 0.2f - Player.AttackSpeed / 50f;
			
			if(Type == AttackType.CLAW2)
				base.MaxCooldown = 1.5f - Player.AttackSpeed / 50f;
			
			if(Type == AttackType.KATAR2)
				base.MaxCooldown = 2.0f - Player.AttackSpeed / 50f;
			
			if(Type == AttackType.BLADE2)
				base.MaxCooldown = 3.5f - Player.AttackSpeed / 50f;
			
			if(Type == AttackType.SLASH)
				base.MaxCooldown = 0.75f - Player.AttackSpeed / 50f;
			
			if(Type == AttackType.KNIFE_SLASH)
				base.MaxCooldown = 0.55f - Player.AttackSpeed / 50f;
			
			if(Type == AttackType.SHOOT)
				base.MaxCooldown = 0.45f - Math.Max(.25f, Player.AttackSpeed / 50f);
			
			if(Type == AttackType.SWING)
				base.MaxCooldown = 0.75f - Player.AttackSpeed / 50f;
			
			if(Type == AttackType.SMASH)
				base.MaxCooldown = 5.25f - Math.Max(.75f, Player.AttackSpeed / 50f);
			
			if(Type == AttackType.TRIPLESHOT)
				base.MaxCooldown = 4.0f- Player.AttackSpeed / 25;

			if (Type == AttackType.KNIFE_LUNGE)
				base.MaxCooldown = 3.0f- Player.AttackSpeed / 25;
			
			if (Type == AttackType.LUNGE)
				base.MaxCooldown = 5.0f- Player.AttackSpeed / 25;
			
			if(Type == AttackType.THROW)
				base.MaxCooldown = 0.45f - Math.Max(.25f, Player.AttackSpeed / 50f);
			
			if(Type == AttackType.THROW_SPECIAL)
				base.MaxCooldown = 4.0f- Player.AttackSpeed / 25;
		}
		
		public override bool MeetsRequirements(SkillsBar Bar, int CastingAbilityCount)
		{
			if(DisableWeapon) return false;
			
			 if(!base.MeetsRequirements(Bar, CastingAbilityCount) || Player.IsAttacking || Player.IsEating || !Player.CanInteract) return false;
			
			if(Type == AttackType.LUNGE)
			{
				return true;//No special requirements
			}
			else
			{
				return true;
			}
		}
		
		private bool _isPressing;
		public override void KeyUp()
		{
			_isPressing = false;
		}
		
		public override void KeyDown(){
			
			_isPressing = true;
			if(Type == AttackType.SLASH || Type == AttackType.SHOOT || Type == AttackType.THROW || Type == AttackType.SWING || Type == AttackType.KNIFE_SLASH 
			   || Type == AttackType.KATAR1 || Type == AttackType.CLAW1 || Type == AttackType.BLADE1){
				Player.Model.LeftWeapon.Attack1(Player.Model);
			}
			if (Type == AttackType.LUNGE || Type == AttackType.TRIPLESHOT || Type == AttackType.THROW_SPECIAL || Type == AttackType.SMASH || Type == AttackType.KNIFE_LUNGE
			    || Type == AttackType.BLADE2 || Type == AttackType.CLAW2 || Type == AttackType.KATAR2){
				Player.Model.LeftWeapon.Attack2(Player.Model);
			}
		}
		
		public override void Update(){
			if(DisableWeapon) return;
			
			if(_isPressing && Cooldown < 0){
				Player.Model.LeftWeapon.Attack1(Player.Model);
				Player.Model.LeftWeapon.ContinousAttack = true;
			}
			else{
				Player.Model.LeftWeapon.ContinousAttack = false;
			}
		}
		
		public override void Draw(){
			if(!Enabled)
				return;
			
			Cooldown -= Time.FrameTimeSeconds;
			if(Cooldown > 0)
				RText.Text = ((int) Cooldown + 1).ToString();
			else
				this.RText.Text = "";
			
			GL.Enable(EnableCap.Blend);
			GL.Disable(EnableCap.DepthTest);
			GL.Disable(EnableCap.CullFace);
			
			Shader.Bind();
			GL.Uniform3(Shader.TintUniform, Tint);
			GL.Uniform2(Shader.ScaleUniform, Scale);
			GL.Uniform2(Shader.PositionUniform, Position);
			GL.Uniform2(Shader.BoolsUniform, new Vector2((Level == 0) ? 1 : 0, (UseMask) ? 1 : 0) );
			GL.Uniform1(Shader.CooldownUniform, this.Cooldown / this.MaxCooldown);
			
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, TexId);
			
			GL.ActiveTexture(TextureUnit.Texture1);
			GL.BindTexture(TextureTarget.Texture2D, SkillsBarMask);
			GL.Uniform1(Shader.MaskUniform, 1);
			
			DrawManager.UIRenderer.SetupQuad();
			DrawManager.UIRenderer.DrawQuad();


            Shader.UnBind();
			
			GL.Disable(EnableCap.Blend);
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.CullFace);
		}
		
		public override string Description {
			get {
				return "";
			}
		}
		
		public enum AttackType{
			SLASH,
			LUNGE,
			SHOOT,
			TRIPLESHOT,
			THROW,
			THROW_SPECIAL,
			SWING,
			SMASH,
			KNIFE_SLASH,
			KNIFE_LUNGE,
			BLADE1,
			BLADE2,
			CLAW1,
			CLAW2,
			KATAR1,
			KATAR2
		}
	}
}
