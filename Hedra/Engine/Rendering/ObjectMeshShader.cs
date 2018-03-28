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
	public class ObjectMeshShader : Shader
	{
		public int TransMatrixUniformLocation;
	    public int MatrixUniformLocation;
		public int LocalRotationLocation;
		public int LocalRotationPointLocation;
		public int LocalPositionLocation;
		public int BeforeLocalRotationLocation;
		public int AnimationPositionLocation;
		public int AnimationRotationLocation;
		public int AnimationRotationPointLocation;
		public int ResolutionUniform;
		public int BakedPositionUniform;
		public int PlayerPositionUniform;
	    public int ShadowDistanceUniform;
		public int ShadowMvpUniform;
	    public int ShadowTexUniform;
	    public int PointLocation;
        public int TintUniform;
	    public int ApplyFogUniform;
	    public int ScaleUniform;
        public int UseShadowsUniform;
	    public int AlphaUniform;
        public int DitherUniform;
	    public int TransPos;
	    public int Time;

        public ObjectMeshShader(string S1, string S2) : base(S1, S2){}
		public ObjectMeshShader(string S1, string S2, string S3) : base(S1, S2, S3){}
		
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
			ApplyFogUniform = GL.GetUniformLocation(ShaderID, "UseFog");
			ScaleUniform = GL.GetUniformLocation(ShaderID, "Scale");
			ResolutionUniform = GL.GetUniformLocation(ShaderID, "res");
			BakedPositionUniform = GL.GetUniformLocation(ShaderID, "BakedPosition");
			PlayerPositionUniform = GL.GetUniformLocation(ShaderID, "PlayerPosition");
			AlphaUniform = GL.GetUniformLocation(ShaderID, "Alpha");
			ShadowTexUniform = GL.GetUniformLocation(ShaderID, "ShadowTex");
			ShadowMvpUniform = GL.GetUniformLocation(ShaderID, "ShadowMVP");
			UseShadowsUniform = GL.GetUniformLocation(ShaderID, "UseShadows");
			DitherUniform = GL.GetUniformLocation(ShaderID, "Dither");
			MatrixUniformLocation = GL.GetUniformLocation(ShaderID, "Matrix");
		    ShadowDistanceUniform = GL.GetUniformLocation(ShaderID, "ShadowDistance");

		}
	}
}
