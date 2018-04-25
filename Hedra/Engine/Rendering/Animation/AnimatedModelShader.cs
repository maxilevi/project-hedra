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
using Hedra.Engine.Rendering.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Hedra.Engine.Rendering.Animation
{
	/// <summary>
	/// Description of AnimatedModelShader.
	/// </summary>
	public class AnimatedModelShader
	{
	    /// <summary>
	    /// Kind of hacky but replaces some shader variables to generate a compatible shader
	    /// </summary>
	    /// <returns></returns>
	    public static Shader GenerateDeathShader()
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
	            Source = sourceV,
	            SourceFinder = () => sourceV
            };

	        var dataG = new ShaderData
	        {
	            Name = "AnimatedModelDeath.geom",
	            Source = sourceG,
	            SourceFinder = () => sourceG
            };

	        var dataF = new ShaderData
	        {
	            Name = "AnimatedModelDeath.frag",
	            Source = sourceF,
                SourceFinder = () => sourceF
            };

            return Shader.Build(dataV, dataG, dataF);
	    }
	}
}
