/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 11/06/2016
 * Time: 03:55 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of TrailShader.
	/// </summary>
	public class TrailShader : Shader
	{
		
		public TrailShader(string s1, string s2) : base(s1,s2){}
		
		public override void GetUniformsLocations(){
		}
	}
}
