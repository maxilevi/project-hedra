/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 25/03/2017
 * Time: 11:15 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of UniformMatrixArray.
	/// </summary>
	public class UniformVector4Array
	{
		private int[] VectorUniforms;
	
		public UniformVector4Array(int ShaderId, string Name, int Size) {
			VectorUniforms = new int[Size];
			for(int i=0; i<Size; i++){
				VectorUniforms[i] = GL.GetUniformLocation(ShaderId, Name + "["+i+"]");
			}
		}
	
		public void LoadVectorArray(Vector4[] Vectors){
			for(int i=0; i<Vectors.Length; i++){
				GL.Uniform4(VectorUniforms[i], Vectors[i]);
			}
		}
	
	}
}