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
using Hedra.Engine.ItemSystem.WeaponSystem;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of Resistance.
	/// </summary>
	public class Puncture : Skill
	{

		private Bow PlayerBow = null;
		private bool WasSet = false;
		
		public Puncture(Vector2 Position, Vector2 Scale, Panel InPanel, LocalPlayer Player) : base(Position, Scale, InPanel, Player) {
			base.TexId = Graphics2D.LoadFromAssets("Assets/Skills/PierceArrows.png");
			base.Passive = true;
			this.Player = Player;
		}
		
		public override void Update()
		{
			if(base.Level == 0)return;

			if(base.Level > 1)
				Player.SkillSystem.SetPoints(this.GetType(), 1);
			
			if(PlayerBow != Player.Model.LeftWeapon){
				if(WasSet){
					PlayerBow.BowModifiers -= PierceModifier;
					WasSet = false;
				}
				if(Player.Model.LeftWeapon is Bow){
					PlayerBow = Player.Model.LeftWeapon as Bow;
					PlayerBow.BowModifiers += PierceModifier;
					WasSet = true;
				}
			}
		}
		
		public void PierceModifier(Projectile ArrowProj){
			ArrowProj.HitEventHandler += delegate(Projectile Sender, Entity Hit) {
				if(Utils.Rng.Next(0, 5) == 0){
					Hit.AddComponent( new BleedingComponent(Hit, Player, 3f, 20f) );
				}
			};
		}
		
		public override string Description {
			get {
				return "Arrows have a high chance to cause bleeding.";
			}
		}
		
		public override void KeyDown(){}
	}
}
