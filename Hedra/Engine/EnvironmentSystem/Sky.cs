/*
 * Author: Zaphyk
 * Date: 27/02/2016
 * Time: 05:30 a.m.
 *
 */

using System.Numerics;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Windowing;
using Hedra.Game;
using Hedra.Numerics;
using SixLabors.ImageSharp;

namespace Hedra.Engine.EnvironmentSystem
{
    /// <summary>
    ///     Description of Skydome.
    /// </summary>
    public sealed class Sky
    {
        private static readonly Shader SkyGradientShader;
        private readonly VBO<Vector3> _gradientDome;
        private readonly SkyOverlay _starsDome;
        private readonly SkyOverlay _sunDome;

        static Sky()
        {
            SkyGradientShader = Shader.Build("Shaders/SkyGradient.vert", "Shaders/SkyGradient.frag");
        }

        public Sky()
        {
            _starsDome = new SkyOverlay(new[]
            {
                "Assets/Sky/stars_right.png",
                "Assets/Sky/stars_left.png",
                "Assets/Sky/stars_top.png",
                null, //"Assets/Sky/stars_bottom.png",
                "Assets/Sky/stars_front.png",
                "Assets/Sky/stars_back.png"
            });
            _sunDome = new SkyOverlay(new[]
            {
                null,
                null,
                "Assets/Sky/sunandmoon_top.png",
                null,
                null,
                null
            });
        }

        public bool Enabled { get; set; } = true;
        public Vector4 TopColor { get; set; } = Color.CornflowerBlue.AsVector4();
        public Vector4 BotColor { get; set; } = Color.LightYellow.AsVector4();

        public void Draw()
        {
            if (!Enabled) return;

            Renderer.Disable(EnableCap.DepthTest);
            Renderer.Disable(EnableCap.Blend);

            SkyGradientShader.Bind();
            SkyGradientShader["topColor"] = TopColor;
            SkyGradientShader["botColor"] = BotColor;
            SkyGradientShader["height"] = GameSettings.Height * (1 - GameManager.Player.View.Pitch * .25f);

            DrawManager.UIRenderer.DrawQuad();

            _starsDome.ColorMultiplier = Vector4.One * (1 - SkyManager.LastDayFactor) * 2f;
            _starsDome.Draw();
            _sunDome.TransformationMatrix =
                Matrix4x4.CreateRotationX(Mathf.Radian * (1 - SkyManager.StackedDaytimeModifier) * 180f);
            _sunDome.ColorMultiplier = Vector4.One * SkyManager.LastDayFactor * 2f;
            _sunDome.Draw();

            SkyGradientShader.Unbind();
            Renderer.Enable(EnableCap.DepthTest);
            Renderer.Enable(EnableCap.CullFace);
        }
    }
}