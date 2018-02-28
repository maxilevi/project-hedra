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
	public class UniformMatrix4Array
	{
		private int[] MatrixUniforms;
	
		public UniformMatrix4Array(int ShaderId, string Name, int Size) {
			MatrixUniforms = new int[Size];
			for(int i=0; i<Size; i++){
				MatrixUniforms[i] = GL.GetUniformLocation(ShaderId, Name + "["+i+"]");
			}
		}
	
		public void LoadMatrixArray(Matrix4[] Matrices){
			for(int i=0; i<Matrices.Length; i++){
				GL.UniformMatrix4(MatrixUniforms[i], false, ref Matrices[i]);
			}
		}
	
	}
}
