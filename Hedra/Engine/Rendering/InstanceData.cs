/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 30/11/2016
 * Time: 05:08 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using OpenTK;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of InstanceData.
	/// </summary>
	public class InstanceData
	{
		public VertexData MeshCache;
		public List<Vector4> Colors;
		public List<float> ExtraData;
		public Matrix4 TransMatrix;
		public Vector4 ColorCache = -Vector4.One;
		public float ExtraDataCache = -1;
	}
}
