/*
 * Author: Zaphyk
 * Date: 07/02/2016
 * Time: 05:01 p.m.
 *
 */
using System;

namespace Hedra.Engine.Scenes
{
	/// <summary>
	/// Description of SceneManager.
	/// </summary>
	public static class SceneManager
	{
		public static GameScene Game;
		public static IScene CurrentScene;
		
		public static void Load(){
			
			Game = new GameScene();
			Log.WriteLine("Loading Game Scene...");
			Game.LoadScene();
			
			SceneManager.SetScene(Game);
			
		}
		
		public static IScene GetSceneById(int id){
			if(Game != null && Game.Id == id)
				return Game;
			return null;
		}
		
		public static void SetScene(IScene s){
			CurrentScene = s;
		}
	}
}
