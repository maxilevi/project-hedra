/*
 * Author: Zaphyk
 * Date: 22/02/2016
 * Time: 12:43 a.m.
 *
 */

using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering
{
    public class FXAAShader : Shader, FXShader
    {
        public int ScaleUniform { get; set; }
        public int PositionUniform { get; set; }
        public int BackGroundUniform { get; set; }
        public int GUIUniform { get; set; }
        public int FlippedUniform { get; set; }
        public int SizeUniform { get; set; }

        public FXAAShader(string s1, string s2) : base(s1, s2){}

        public override void GetUniformsLocations()
        {
            ScaleUniform = GL.GetUniformLocation(ShaderID, "Scale");
            PositionUniform = GL.GetUniformLocation(ShaderID, "Position");
            GUIUniform = GL.GetUniformLocation(ShaderID, "Texture");
            BackGroundUniform = GL.GetUniformLocation(ShaderID, "Background");
            FlippedUniform = GL.GetUniformLocation(ShaderID, "Flipped");
            SizeUniform = GL.GetUniformLocation(ShaderID, "Size");
        }
    }
}