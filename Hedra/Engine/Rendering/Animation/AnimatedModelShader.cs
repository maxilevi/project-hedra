﻿/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 25/03/2017
 * Time: 10:27 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Shaders;
using Newtonsoft.Json.Serialization;

namespace Hedra.Engine.Rendering.Animation
{
	/// <summary>
	/// Description of AnimatedModelShader.
	/// </summary>
	internal class AnimatedModelShader
	{
	    /// <summary>
	    /// Kind of hacky but replaces some shader variables to generate a compatible shader
	    /// </summary>
	    /// <returns></returns>
	    public static Shader GenerateDeathShader()
	    {
	        string VertexSource()
	        {
	            string sourceV = AssetManager.ReadShader("Shaders/AnimatedModel.vert");
	            if (CompatibilityManager.SupportsGeometryShaders)
	            {
	                sourceV = sourceV.Replace("pass_color", "pass_colors");
	                sourceV = sourceV.Replace("pass_position", "pass_positions");
	                sourceV = sourceV.Replace("pass_normal", "pass_normals");
	                sourceV = sourceV.Replace("pass_lightDiffuse", "pass_lightDiffuses");
	            }
	            return sourceV;
	        }

	        string GeometrySource()
	        {
	            return AssetManager.ReadShader("Shaders/AnimatedModelDeath.geom");
	        }

	        string FragmentSource()
	        {
	            string sourceF = AssetManager.ReadShader("Shaders/AnimatedModel.frag");
	            if (CompatibilityManager.SupportsGeometryShaders)
	            {
	                sourceF = sourceF.Replace("pass_visibility);", "1.0);");
	            }
	            return sourceF;
	        }

	        var dataV = new ShaderData
	        {
	            Name = "AnimatedModelDeath.vert",
	            Source = VertexSource(),
	            SourceFinder = VertexSource
            };

	        var dataG = new ShaderData
	        {
	            Name = "AnimatedModelDeath.geom",
	            Source = GeometrySource(),
	            SourceFinder = GeometrySource
            };

	        var dataF = new ShaderData
	        {
	            Name = "AnimatedModelDeath.frag",
	            Source = FragmentSource(),
                SourceFinder = FragmentSource
            };

            return Shader.Build(dataV, CompatibilityManager.SupportsGeometryShaders ? dataG : null, dataF);
	    }
	}
}
