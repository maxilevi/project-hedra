/*
 * Author: Zaphyk
 * Date: 20/02/2016
 * Time: 07:53 p.m.
 *
 */
using System;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering.Effects
{

	public static class BlockShaders
	{
		public static WaterShader WaterShader = new WaterShader("Shaders/Water.vert", "Shaders/Water.frag");
		public static StaticShader StaticShader = new StaticShader("Shaders/Static.vert","Shaders/Static.frag");
	}
}
