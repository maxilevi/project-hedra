using System.Numerics;
using Hedra.Core;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Windowing;
using Hedra.Game;
using Hedra.Numerics;

namespace Hedra.Engine.Player.QuestSystem
{
    public static class EntityRenderer
    {
        private static float _rotation;

        private static FBO Framebuffer => InventoryItemRenderer.Framebuffer;

        public static uint Draw(AnimatedModel Model, float ModelHeight)
        {
            var previousBound = Renderer.ShaderBound;
            var previousFBO = Renderer.FBOBound;
            Framebuffer.Bind();

            var meshPrematureCulling = Model.PrematureCulling;
            var applyFog = Model.ApplyFog;
            Model.PrematureCulling = false;
            Model.ApplyFog = false;

            var currentDayColor = ShaderManager.LightColor;
            ShaderManager.SetLightColorInTheSameThread(Vector3.One);

            var previousShadows = GameSettings.GlobalShadows;
            GameSettings.GlobalShadows = false;

            const float aspect = 1.33f;
            var projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(40 * Mathf.Radian, aspect, 1, 1024f);
            Renderer.LoadProjection(projectionMatrix);
            var offset = Vector3.UnitY * ModelHeight * .5f;
            var eyeOffset = offset + Vector3.UnitZ * 10;
            var rotMat = Matrix4x4.CreateRotationY(_rotation * Mathf.Radian);
            var lookAt = Matrix4x4.CreateLookAt(
                Model.Position + Vector3.Transform(eyeOffset, rotMat),
                Model.Position + Vector3.Transform(offset, rotMat), Vector3.UnitY
            );
            Renderer.LoadModelView(lookAt);

            Renderer.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Renderer.Enable(EnableCap.DepthTest);
            Renderer.Disable(EnableCap.Blend);

            Model.UpdateJointTransforms();
            Model.DrawModel(lookAt * projectionMatrix, lookAt);

            ShaderManager.SetLightColorInTheSameThread(currentDayColor);
            GameSettings.GlobalShadows = previousShadows;
            Renderer.BindFramebuffer(FramebufferTarget.Framebuffer, previousFBO);
            Renderer.BindShader(previousBound);
            Renderer.Disable(EnableCap.DepthTest);
            Renderer.Enable(EnableCap.Blend);
            Renderer.Viewport(0, 0, GameSettings.DeviceWidth, GameSettings.DeviceHeight);
            Model.PrematureCulling = meshPrematureCulling;
            Model.ApplyFog = applyFog;
            return Framebuffer.TextureId[0];
        }

        public static void Update()
        {
            _rotation += Time.DeltaTime * 25f;
        }
    }
}