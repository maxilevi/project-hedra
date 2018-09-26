/*
 * Author: Zaphyk
 * Date: 07/02/2016
 * Time: 11:33 p.m.
 *
 */
using System;
using Hedra.Engine.Rendering;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.Management
{
	/// <summary>
	/// Description of OcclusionCulling.
	/// </summary>
	public static class OcclusionCulling
	{
		public static void StartQueries(Occludable[] Occludables){
			Renderer.ColorMask(false, false, false, false);
			Renderer.DepthMask(false);
			for(int i = 0; i < Occludables.Length; i++){
				Occludables[i].DrawQuery();
			}
			Renderer.ColorMask(true, true, true, true);
			Renderer.DepthMask(true);
		}
	}
}
