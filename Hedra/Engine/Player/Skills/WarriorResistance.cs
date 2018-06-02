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
	    private bool _set;
		private float _addonHealth;
		private int _previousLevel;
		private int _previousSkillLevel;
		public override uint TexId =>  Graphics2D.LoadFromAssets("Assets/Skills/Health.png");
		public override bool Passive =>  true;
		
		public float HealthFormula(bool Clamp = false)
		{
		    return Clamp ? 12 * Math.Max(base.Level, 1) : 12 * base.Level;
		}
		
		public override void Update()
		{
			if(_previousLevel != this.Player.Level || _previousSkillLevel != this.Level) _set = false;
			
			if(!_set){
				_previousSkillLevel = (int) this.Level;
				_previousLevel = this.Player.Level;
				Player.AddonHealth -= _addonHealth;
				_addonHealth = this.HealthFormula() * _previousLevel;
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
		
		public override string Description => $"Grants +{this.HealthFormula(true):0.0} HP.";

	    public override void Use(){}
	}
}
