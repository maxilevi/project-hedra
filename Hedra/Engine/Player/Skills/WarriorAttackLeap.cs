/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:13 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.EntitySystem;
using OpenTK;
using System.Collections.Generic;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of WeaponThrow.
	/// </summary>
	public class AttackLeap : Skill
	{
		
		public AttackLeap(Vector2 Position, Vector2 Scale, Panel InPanel, LocalPlayer Player) : base(Position, Scale, InPanel, Player) {
			base.TexId = Graphics2D.LoadFromAssets("Assets/Skills/AttackLeap.png");
			base.ManaCost = 85;
			base.MaxCooldown = 8.5f;
		}

		
		public override void KeyDown(){
			base.MaxCooldown = 8.5f - base.Level * .5f;
			Player.IsCasting = true;
			Casting = true;
			Player.IsAttacking = true;
		}
		
		public override void Update(){
			if(Player.IsCasting && Casting){
			}
		}
		
		public override string Description {
			get {
				return "Jumps and attack the target with an intense strike.";
			}
		}
	}
}