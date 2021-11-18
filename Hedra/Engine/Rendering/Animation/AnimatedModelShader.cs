/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 25/03/2017
 * Time: 10:27 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Rendering.Shaders;

namespace Hedra.Engine.Rendering.Animation
{
    /// <summary>
    ///     Description of AnimatedModelShader.
    /// </summary>
    public class AnimatedModelShader
    {
        /// <summary>
        ///     Kind of hacky but replaces some shader variables to generate a compatible shader
        /// </summary>
        /// <returns></returns>
        public static Shader GenerateDeathShader()
        {
            string VertexSource()
            {
                var sourceV = AssetManager.ReadShader("Shaders/AnimatedModel.vert");
                if (CompatibilityManager.SupportsGeometryShaders)
                {
                    sourceV = sourceV.Replace("pass_color", "pass_colors");
                    sourceV = sourceV.Replace("pass_position", "pass_positions");
                    sourceV = sourceV.Replace("pass_normal", "pass_normals");
                    sourceV = sourceV.Replace("pass_lightDiffuse", "pass_lightDiffuses");
                    sourceV = sourceV.Replace("pass_height", "pass_heights");
                    sourceV = sourceV.Replace("base_vertex_positions", "base_vertex_positions");
                    sourceV = sourceV.Replace("pass_botColor", "pass_botColors");
                    sourceV = sourceV.Replace("pass_topColor", "pass_topColors");
                    sourceV = sourceV.Replace("pass_coords", "pass_coordss");
                }

                return sourceV;
            }

            string GeometrySource()
            {
                return AssetManager.ReadShader("Shaders/AnimatedModelDeath.geom");
            }

            string FragmentSource()
            {
                var sourceF = AssetManager.ReadShader("Shaders/AnimatedModel.frag");
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