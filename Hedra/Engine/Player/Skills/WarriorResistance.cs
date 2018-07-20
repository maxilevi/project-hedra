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
	internal class Resistance : PassiveSkill
	{
	    private bool _set;
		private float _addonHealth;
		private int _previousLevel;
		private int _previousSkillLevel;

		private float HealthFormula(bool Clamp = false)
		{
		    return Clamp ? 12 * Math.Max(Level, 1) : 12 * Level;
		}
		
		protected override void Change()
		{
			if(_previousLevel != Player.Level || _previousSkillLevel != Level) _set = false;
			
			if(!_set){
				_previousSkillLevel = Level;
				_previousLevel = Player.Level;
				Player.AddonHealth -= _addonHealth;
				_addonHealth = HealthFormula() * _previousLevel;
				Player.AddonHealth += _addonHealth;
				if(Player.Health > Player.MaxHealth) Player.Health = Player.MaxHealth;
				_set = true;
			}
		}
		
		public override void Load()
		{
			Player.AddonHealth += _addonHealth;
		}
		
		public override void Unload()
		{
			Player.AddonHealth -= _addonHealth;
		}

		protected override int MaxLevel => 10;
		
		public override string Description => $"Grants +{HealthFormula(true):0.0} HP.";
		
		public override uint TexId =>  Graphics2D.LoadFromAssets("Assets/Skills/Health.png");
	}
}
