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

namespace Hedra.Engine.Player.Skills
{
	internal class Agility : PassiveSkill
	{
		private float StaminaFormula(bool clamp = false)
		{
			if(clamp)
				return -1f * Math.Max(Level,1);
			return -1f * Level;
		}

		protected override void Change()
		{
			Player.DodgeCost = 25 + StaminaFormula();
		}
		
		protected override int MaxLevel => 10;
		public override uint TexId => Graphics2D.LoadFromAssets("Assets/Skills/Agility.png");
		public override string Description => $"Dodging costs {-StaminaFormula(true)} less stamina.";
	}
}
