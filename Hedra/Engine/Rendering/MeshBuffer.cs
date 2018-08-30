/*
 * Author: Zaphyk
 * Date: 20/02/2016
 * Time: 08:34 p.m.
 *
 */
using System;
using System.Collections.Generic;
using Hedra.Engine.Rendering.Effects;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Linq;

namespace Hedra.Engine.Rendering
{
	public abstract class MeshBuffer
	{
	    public VBO<Vector3> Vertices;
		public VBO<Vector4> Colors;
		public VBO<uint> Indices;
		public VBO<Vector3> Normals;
		public VAO<Vector3, Vector4, Vector3> Data;

		public abstract void Draw();

		public void Dispose()
		{
			Vertices?.Dispose();
			Colors?.Dispose();
			Indices?.Dispose();
			Normals?.Dispose();
			Data?.Dispose();
		}
	}
}