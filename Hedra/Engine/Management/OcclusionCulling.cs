/*
 * Author: Zaphyk
 * Date: 07/02/2016
 * Time: 11:33 p.m.
 *
 */
using System;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Management
{
	/// <summary>
	/// Description of OcclusionCulling.
	/// </summary>
	internal static class OcclusionCulling
	{
		public static void StartQueries(Occludable[] Occludables){
			GL.ColorMask(false, false, false, false);
			GL.DepthMask(false);
			for(int i = 0; i < Occludables.Length; i++){
				Occludables[i].DrawQuery();
			}
			GL.ColorMask(true, true, true, true);
			GL.DepthMask(true);
		}
	}
}
