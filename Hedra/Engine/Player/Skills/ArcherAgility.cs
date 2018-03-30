/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:14 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of Resistance.
	/// </summary>
	public class Agility : Skill
	{
		private float BonusStamina = 0;
		private int PrevLevel = 0;
		private int PrevSkillLevel = 0;
		
		public Agility(Vector2 Position, Vector2 Scale, Panel InPanel, LocalPlayer Player) : base(Position, Scale, InPanel, Player) {
			base.TexId = Graphics2D.LoadFromAssets("Assets/Skills/Agility.png");
			base.Passive = true;
			this.Player = Player;
		}
		
		public float StaminaFormula(bool clamp = false){
			if(clamp)
				return -1f * Math.Max(base.Level,1);
			else
				return -1f * base.Level;
		}
		
		public override void Update()
		{
			if(base.Level > 10)
				Player.AbilityTree.SetPoints(this.GetType(), 10);
			
			Player.DodgeCost = 25 + StaminaFormula();
		
		}
		
		public override string Description {
			get {
				return "Dodging costs "+(-StaminaFormula(true))+" less stamina.";
			}
		}
		
		public override void KeyDown(){}
	}
}
