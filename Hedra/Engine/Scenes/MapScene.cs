/*
 * Author: Zaphyk
 * Date: 17/04/2016
 * Time: 04:52 p.m.
 *
 */
using System;

namespace Hedra.Engine.Scenes
{
	/// <summary>
	/// Description of MapScene.
	/// </summary>
	public class MapScene : IScene
	{
		public int Id {get; set;}
		
		public MapScene()
		{
			Id = 2;
		}
		
		public void LoadScene(){
		
		}
	}
}
