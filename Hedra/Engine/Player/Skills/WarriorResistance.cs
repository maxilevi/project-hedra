/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:14 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Player.Skills
{
	/// <summary>
	/// Description of Resistance.
	/// </summary>
	public class Resistance : BaseSkill
	{
		private bool Set = false;
		private float AddonHealth = 0;
		private int PrevLevel = 0;
		private int PrevSkillLevel = 0;
		
		public Resistance(Vector2 Position, Vector2 Scale, Panel InPanel, LocalPlayer Player) : base(Position, Scale, InPanel, Player) {
			base.TexId = Graphics2D.LoadFromAssets("Assets/Skills/Health.png");
			base.Passive = true;
		}
		
		public float HealthFormula(bool clamp = false){
			if(clamp)
				return 4*Math.Max(base.Level,1)*.6f;
			else
				return 4*base.Level*.6f;
		}
		
		public override void Update()
		{
			if(PrevLevel != this.Player.Level || PrevSkillLevel != this.Level) Set = false;
			
			if(!Set){
				PrevSkillLevel = (int) this.Level;
				PrevLevel = this.Player.Level;
				Player.AddonHealth -= AddonHealth;
				AddonHealth = this.HealthFormula() * PrevLevel;
				Player.AddonHealth += AddonHealth;
				if(Player.Health > Player.MaxHealth) Player.Health = Player.MaxHealth;
				Set = true;
			}
		}
		
		public override void LoadBuffs()
		{
			Player.AddonHealth += AddonHealth;
		}
		
		public override void UnloadBuffs()
		{
			Player.AddonHealth -= AddonHealth;
		}
		
		public override string Description {
			get {
				return "Grants +"+this.HealthFormula(true)+" HP.";
			}
		}
		
		public override void KeyDown(){}
	}
}
