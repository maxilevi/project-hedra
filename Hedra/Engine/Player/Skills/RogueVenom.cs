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
using System.Linq;
using System.Collections.Generic;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Player.Skills;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of Resistance.
	/// </summary>
	public class Venom : BaseSkill
	{
		private int PreviousLevel = 0;
		public Venom() : base() {
			base.TexId = Graphics2D.LoadFromAssets("Assets/Skills/Venom.png");
			base.Passive = true;
		}
		
		public override void Update()
		{
			if(base.Level == 0)return;
			
			if(Player.SearchComponent<PoisonousComponent>() == null)
				Player.AddComponent( new PoisonousComponent(Player) );
			
			if(PreviousLevel != base.Level){
				PreviousLevel = base.Level;
				PoisonousComponent Poison = Player.SearchComponent<PoisonousComponent>();
				Poison.Chance = (int) (100 * (Math.Min(.4f, base.Level * .05f) + .2f));
				Poison.Damage = base.Level * 7.5f + 20f;
				Poison.Duration = 8f - Math.Min(4f, base.Level * .5f);
			}
			
		}
		
		public override string Description {
			get {
				return "Your attacks have a chance to apply poison.";
			}
		}
		
		public override void Use(){}
	}
}
