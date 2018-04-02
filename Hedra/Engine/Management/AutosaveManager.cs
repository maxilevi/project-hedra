/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 21/01/2017
 * Time: 02:38 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Player;

namespace Hedra.Engine.Management
{
	/// <summary>
	/// Description of AutosaveManager.
	/// </summary>
	public static class AutosaveManager
	{
		public static int TimePerSave = 30;
		private static float PassedTime;
		public static void Update(){
			if(!GameSettings.Autosave || !Constants.CHARACTER_CHOOSED) return;
			
			PassedTime += Time.FrameTimeSeconds;
			if(PassedTime >= TimePerSave){
				AutosaveManager.Save();
				PassedTime = 0;
			}
		}
		
		public static void Save(){
			LocalPlayer.Instance.UnLoad();
			
			for(var i = 0; i < Scenes.SceneManager.Game.Player.Toolbar.Skills.Length; i++)
				Scenes.SceneManager.Game.Player.Toolbar.Skills[i].UnloadBuffs();
			
			DataManager.SavePlayer( DataManager.DataFromPlayer(Scenes.SceneManager.Game.Player) );
			
			for(var i = 0; i < Scenes.SceneManager.Game.Player.Toolbar.Skills.Length; i++)
				Scenes.SceneManager.Game.Player.Toolbar.Skills[i].LoadBuffs();
			
			LocalPlayer.Instance.Load();
		}
	}
}
