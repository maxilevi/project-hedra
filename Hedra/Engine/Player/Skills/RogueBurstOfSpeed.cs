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
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Rendering.Animation;
using OpenTK;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of WeaponThrow.
	/// </summary>
	public class BurstOfSpeed : BaseSkill
	{	
		public BurstOfSpeed(Vector2 Position, Vector2 Scale, Panel InPanel, LocalPlayer Player) : base(Position, Scale, InPanel, Player) {
			base.TexId = Graphics2D.LoadFromAssets("Assets/Skills/BurstOfSpeed.png");
			base.ManaCost = 80f;
			base.MaxCooldown = 14f;
		}
		
		public override void KeyDown(){
			base.MaxCooldown = 14f;
			CoroutineManager.StartCoroutine(SpeedTime);
		}
		
		private IEnumerator SpeedTime(){
			float passedTime = 4f + Math.Min(base.Level * .5f, 2f);
		    float pTime = 0;
		    Player.AddBonusSpeedWhile(Math.Min(base.Level * .1f, 1), () => pTime < passedTime);

            while (pTime < passedTime){							
				pTime += Time.ScaledFrameTimeSeconds;
				yield return null;
			}			
		}
		
		public override void Update(){}
		
		public override string Description {
			get {
				return "Temporarily obtain a speed burst.";
			}
		}
	}
}