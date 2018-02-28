/*
 * Author: Zaphyk
 * Date: 07/02/2016
 * Time: 05:02 p.m.
 *
 */
using System;

namespace Hedra.Engine.Scenes
{

	public interface IScene
	{
		int Id {get; set;}
		
		void LoadScene();
		
	}
}
