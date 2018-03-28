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
			
			if(Type == AttackType.Katar1)
				base.TexId = Katar1Icon;
			
			if (Type == AttackType.Katar2)
				base.TexId = Katar2Icon;
			
			if(Type == AttackType.Blade1)
				base.TexId = Blade1Icon;
			
			if (Type == AttackType.Blade2)
				base.TexId = Blade2Icon;
			
			if(Type == AttackType.Claw1)
				base.TexId = Claw1Icon;
			
			if (Type == AttackType.Claw2)
				base.TexId = Claw2Icon;
			
			
			if(Type == AttackType.Slash)
				base.TexId = SlashIcon;
			
			if (Type == AttackType.Lunge)
				base.TexId = LungeIcon;
			
			if(Type == AttackType.KnifeSlash)
				base.TexId = SlashKnifeIcon;
			
			if (Type == AttackType.KnifeLunge)
				base.TexId = LungeKnifeIcon;
			
			if (Type == AttackType.Shoot)
				base.TexId = ShootIcon;
			
			if(Type == AttackType.Tripleshot)
				base.TexId = TripleShotIcon;
			
			if (Type == AttackType.Throw)
				base.TexId = ThrowIcon;
			
			if(Type == AttackType.ThrowSpecial)
				base.TexId = ThrowSpecialIcon;
			
			if (Type == AttackType.Swing)
			{
			    base.TexId = Player.Inventory.MainWeapon.EquipmentType == EquipmentType.Hammer.ToString() ? SwingHammerIcon : SwingAxeIcon;
			}
			
			if(Type == AttackType.Smash)
			{
			    base.TexId = Player.Inventory.MainWeapon.EquipmentType == EquipmentType.Hammer.ToString() ? SmashHammerIcon : SmashAxeIcon;
			}
		    base.MaxCooldown = 0.25f;
		}
		
		public override bool MeetsRequirements(SkillsBar Bar, int CastingAbilityCount)
		{
			if(DisableWeapon) return false;
			
			 if(!base.MeetsRequirements(Bar, CastingAbilityCount) || Player.IsAttacking || Player.IsEating || !Player.CanInteract) return false;
			
			if(Type == AttackType.Lunge)
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
			if(Type == AttackType.Slash || Type == AttackType.Shoot || Type == AttackType.Throw || Type == AttackType.Swing || Type == AttackType.KnifeSlash 
			   || Type == AttackType.Katar1 || Type == AttackType.Claw1 || Type == AttackType.Blade1){
				Player.Model.LeftWeapon.Attack1(Player);
			}
			if (Type == AttackType.Lunge || Type == AttackType.Tripleshot || Type == AttackType.ThrowSpecial || Type == AttackType.Smash || Type == AttackType.KnifeLunge
			    || Type == AttackType.Blade2 || Type == AttackType.Claw2 || Type == AttackType.Katar2){
				Player.Model.LeftWeapon.Attack2(Player);
			}
		}
		
		public override void Update(){
			if(DisableWeapon) return;
			
			if(_isPressing && Cooldown < 0){
				Player.Model.LeftWeapon.Attack1(Player);
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
		
		public override string Description => string.Empty;

	    public enum AttackType{
			Slash,
			Lunge,
			Shoot,
			Tripleshot,
			Throw,
			ThrowSpecial,
			Swing,
			Smash,
			KnifeSlash,
			KnifeLunge,
			Blade1,
			Blade2,
			Claw1,
			Claw2,
			Katar1,
			Katar2
		}
	}
}
