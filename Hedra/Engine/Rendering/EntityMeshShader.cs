/*
 * Author: Zaphyk
 * Date: 25/03/2016
 * Time: 12:36 a.m.
 *
 */
using System;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of EntityMeshShader.
	/// </summary>
	public class EntityMeshShader : Shader
	{
		public int TransMatrixUniformLocation, MatrixUniformLocation;
		public int PointLocation;
		public int LocalRotationLocation;
		public int LocalRotationPointLocation;
		public int LocalPositionLocation;
		public int BeforeLocalRotationLocation;
		public int AnimationPositionLocation;
		public int AnimationRotationLocation;
		public int AnimationRotationPointLocation;
		public int TransPos;
		public int Time;
		public int TintUniform;
		public int UseFogUniform;
		public int ScaleUniform;
		public int OutlineUniform;
		public int ResolutionUniform;
		public int BakedPositionUniform;
		public int PlayerPositionUniform;
		public int AlphaUniform;
	    public int ShadowDistanceUniform;
		public int ShadowMVPUniform, ShadowTexUniform, UseShadowsUniform;
		public int DitherUniform;
		public EntityMeshShader(string s1, string s2) : base(s1, s2){}
		public EntityMeshShader(string s1, string s2, string s3) : base(s1, s2, s3){}
		
		public override void GetUniformsLocations(){
			TransMatrixUniformLocation = GL.GetUniformLocation(ShaderID, "TransMatrix");
			PointLocation = GL.GetUniformLocation(ShaderID, "Point");
			LocalRotationLocation = GL.GetUniformLocation(ShaderID, "LocalRotation");
			LocalRotationPointLocation = GL.GetUniformLocation(ShaderID, "LocalRotationPoint");
			LocalPositionLocation = GL.GetUniformLocation(ShaderID, "LocalPosition");
			BeforeLocalRotationLocation = GL.GetUniformLocation(ShaderID, "BeforeLocalRotation");
			AnimationPositionLocation = GL.GetUniformLocation(ShaderID, "AnimationPosition");
			AnimationRotationLocation = GL.GetUniformLocation(ShaderID, "AnimationRotation");
			AnimationRotationPointLocation = GL.GetUniformLocation(ShaderID, "AnimationRotationPoint");
			TransPos = GL.GetUniformLocation(ShaderID, "TransPos");
			TintUniform = GL.GetUniformLocation(ShaderID, "Tint");
			UseFogUniform = GL.GetUniformLocation(ShaderID, "UseFog");
			ScaleUniform = GL.GetUniformLocation(ShaderID, "Scale");
			OutlineUniform = GL.GetUniformLocation(ShaderID, "Outline");
			ResolutionUniform = GL.GetUniformLocation(ShaderID, "res");
			BakedPositionUniform = GL.GetUniformLocation(ShaderID, "BakedPosition");
			PlayerPositionUniform = GL.GetUniformLocation(ShaderID, "PlayerPosition");
			AlphaUniform = GL.GetUniformLocation(ShaderID, "Alpha");
			ShadowTexUniform = GL.GetUniformLocation(ShaderID, "ShadowTex");
			ShadowMVPUniform = GL.GetUniformLocation(ShaderID, "ShadowMVP");
			UseShadowsUniform = GL.GetUniformLocation(ShaderID, "UseShadows");
			DitherUniform = GL.GetUniformLocation(ShaderID, "Dither");
			MatrixUniformLocation = GL.GetUniformLocation(ShaderID, "Matrix");
		    ShadowDistanceUniform = GL.GetUniformLocation(ShaderID, "ShadowDistance");

		}
	}
}
