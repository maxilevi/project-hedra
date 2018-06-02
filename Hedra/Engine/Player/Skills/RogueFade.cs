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
	public class Fade : BaseSkill
	{
		
		public Fade() : base() 
		{
			base.TexId = Graphics2D.LoadFromAssets("Assets/Skills/Fade.png");
			base.ManaCost = 80f;
			base.MaxCooldown = 16f;
		}
		
		public override void Use(){
			base.MaxCooldown = 20f;
			CoroutineManager.StartCoroutine(FadeTime);
		}
		
		private IEnumerator FadeTime(){
			float PassedTime = 8f + Math.Min(base.Level * .75f, 10f), PTime = 0;
			Sound.SoundManager.PlayUISound(Sound.SoundType.DarkSound, 1f, .25f);
			while(PTime < PassedTime){
				
				Player.Model.Alpha = .4f;
				Player.Model.BaseTint = -new Vector4(.8f,.8f,.8f,0);
				Player.IsInvisible = true;
				
				if(Player.IsAttacking || Player.IsCasting){
					Player.Model.Alpha = 1;
					Player.Model.BaseTint = Vector4.Zero;
					Player.IsInvisible = false;
					yield break;
				}
				
				PTime += Engine.Time.ScaledFrameTimeSeconds;
				yield return null;
			}
			
			Player.Model.Alpha = 1;
			Player.Model.BaseTint = Vector4.Zero;
			Player.IsInvisible = false;
		}
		
		public override void Update(){}
		
		public override string Description {
			get {
				return "Temporarily hide from your enemies.";
			}
		}
	}
}