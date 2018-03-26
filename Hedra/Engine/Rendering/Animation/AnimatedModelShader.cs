/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 25/03/2017
 * Time: 10:27 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Hedra.Engine.Rendering.Animation
{
	/// <summary>
	/// Description of AnimatedModelShader.
	/// </summary>
	public class AnimatedModelShader : Shader
	{
		public int ProjectionViewUniform;
	    public int ViewUniform;
	    public UniformMatrix4Array Mat4Uniform;
		public int ShadowTexUniform;
	    public int UseShadowsUniform;
	    public int ShadowMvpUniform;
	    public int PlayerPositionUniform;
	    public int UseFogUniform;
	    public int AlphaUniform;
	    public int TintUniform;
	    public int DisposeTimeUniform;
	    public int ShadowTransitionUniform;
	    public AnimatedModelShader(string S1, string S2) : base(S1,S2){}
	    public AnimatedModelShader(string S1, string S2, string S3) : base(S1, S2, S3) { }
	    public AnimatedModelShader(ShaderData DataV, ShaderData DataG, ShaderData DataF) : base(DataV, DataG, DataF) { }

        public override void GetUniformsLocations()
		{
			base.GetUniformsLocations();
			this.Mat4Uniform = new UniformMatrix4Array(ShaderID, "jointTransforms", GeneralSettings.MaxJoints);
			this.ProjectionViewUniform = GL.GetUniformLocation(ShaderID, "projectionViewMatrix");
		    this.ViewUniform = GL.GetUniformLocation(ShaderID, "viewMatrix");
            this.PlayerPositionUniform = GL.GetUniformLocation(ShaderID, "PlayerPosition");
			this.AlphaUniform = GL.GetUniformLocation(ShaderID, "Alpha");
			this.TintUniform = GL.GetUniformLocation(ShaderID, "Tint");
			this.ShadowTexUniform = GL.GetUniformLocation(ShaderID, "ShadowTex");
			this.ShadowMvpUniform = GL.GetUniformLocation(ShaderID, "ShadowMVP");
			this.UseShadowsUniform = GL.GetUniformLocation(ShaderID, "UseShadows");
			this.UseFogUniform = GL.GetUniformLocation(ShaderID, "UseFog");
		    this.DisposeTimeUniform = GL.GetUniformLocation(ShaderID, "disposeTime");
		    this.ShadowTransitionUniform = GL.GetUniformLocation(ShaderID, "ShadowTransition");
        }

	    /// <summary>
	    /// Kind of hacky but replaces some shader variables to generate a compatible shader
	    /// </summary>
	    /// <returns></returns>
	    public static AnimatedModelShader GenerateDeathShader()
	    {

            //Maybe add some dynamic retrieving?
	        string sourceV = AssetManager.ReadShader("Shaders/AnimatedModel.vert");
	        string sourceG = AssetManager.ReadShader("Shaders/AnimatedModelDeath.geom");
	        string sourceF = AssetManager.ReadShader("Shaders/AnimatedModel.frag");


	        sourceV = sourceV.Replace("pass_color", "pass_colors");
	        sourceV = sourceV.Replace("pass_position", "pass_positions");
	        sourceV = sourceV.Replace("pass_normal", "pass_normals");
            sourceF = sourceF.Replace("pass_visibility);", "1.0);");

            var dataV = new ShaderData
	        {
	            Name = "AnimatedModelDeath.vert",
	            Source = sourceV
	        };

	        var dataG = new ShaderData
	        {
	            Name = "AnimatedModelDeath.geom",
	            Source = sourceG
            };

	        var dataF = new ShaderData
	        {
	            Name = "AnimatedModelDeath.frag",
	            Source = sourceF
            };

            return new AnimatedModelShader(dataV, dataG, dataF);
	    }
	}
}
