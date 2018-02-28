/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 21/06/2016
 * Time: 06:19 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Hedra.Engine.Scenes
{
	/// <summary>
	/// Description of MenuScene.
	/// </summary>
	public class MenuScene : IScene
	{
		public int Id {get; set;}
		public MenuScene()
		{
			Id = 3;
		}
		
		public void LoadScene(){
		
		}
	}
}
