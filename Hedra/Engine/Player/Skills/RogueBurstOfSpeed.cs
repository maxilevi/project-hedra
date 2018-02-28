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
using Hedra.Engine.Rendering.Animation;
using OpenTK;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of WeaponThrow.
	/// </summary>
	public class BurstOfSpeed : Skill
	{
	
		private float PreviousSpeed = 0;		
		public BurstOfSpeed(Vector2 Position, Vector2 Scale, Panel InPanel, LocalPlayer Player) : base(Position, Scale, InPanel, Player) {
			base.TexId = Graphics2D.LoadFromAssets("Assets/Skills/BurstOfSpeed.png");
			base.ManaCost = 80f;
			base.MaxCooldown = 14f;
		}
		
		public override void KeyDown(){
			base.MaxCooldown = 14f;
			PreviousSpeed = Player.Speed;
			CoroutineManager.StartCoroutine(SpeedTime);
		}
		
		private IEnumerator SpeedTime(){
			float PassedTime = 4f + Math.Min(base.Level * .5f, 2f), PTime = 0;
			while(PTime < PassedTime){
				
				Player.Speed = 1.25f + Math.Min(base.Level * .1f, 1);
				
				PTime += Engine.Time.ScaledFrameTimeSeconds;
				yield return null;
			}
			
			Player.Speed = PreviousSpeed;
		}
		
		public override void Update(){}
		
		public override string Description {
			get {
				return "Temporarily obtain a speed burst.";
			}
		}
	}
}