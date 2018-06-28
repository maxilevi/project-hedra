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
	internal static class AutosaveManager
	{
		public static int TimePerSave = 30;
		private static float PassedTime;
		public static void Update(){
			if(!GameSettings.Autosave || GameManager.InStartMenu) return;
			
			PassedTime += Time.FrameTimeSeconds;
			if(PassedTime >= TimePerSave){
				AutosaveManager.Save();
				PassedTime = 0;
			}
		}
		
		public static void Save(){
			LocalPlayer.Instance.UnLoad();
			
			for(var i = 0; i < GameManager.Player.Toolbar.Skills.Length; i++)
				GameManager.Player.Toolbar.Skills[i].Unload();
			
			DataManager.SavePlayer( DataManager.DataFromPlayer(GameManager.Player) );
			
			for(var i = 0; i < GameManager.Player.Toolbar.Skills.Length; i++)
				GameManager.Player.Toolbar.Skills[i].Load();
			
			LocalPlayer.Instance.Load();
		}
	}
}
