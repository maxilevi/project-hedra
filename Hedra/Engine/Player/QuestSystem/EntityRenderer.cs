using Hedra.Core;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Game;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.Player.QuestSystem
{
    public static class EntityRenderer
    {
        private static float _rotation;
        
        public static uint Draw(AnimatedModel Model)
        {
            Renderer.PushShader();
            Renderer.PushFBO();
            Framebuffer.Bind();

            var meshPrematureCulling = Model.PrematureCulling;
            Model.PrematureCulling = false;

            var currentDayColor = ShaderManager.LightColor;
            ShaderManager.SetLightColorInTheSameThread(Vector3.One);

            const float aspect = 1.33f;
            var projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(40 * Mathf.Radian, aspect, 1, 1024f);
            Renderer.LoadProjection(projectionMatrix);
            var offset = Vector3.UnitY * 3;
            var eyeOffset = offset + Vector3.UnitZ * 10;
            var rotMat = Matrix4.CreateRotationY(_rotation * Mathf.Radian);
            var lookAt = Matrix4.LookAt(
                Model.Position + Vector3.TransformPosition(eyeOffset, rotMat),
                Model.Position + Vector3.TransformPosition(offset, rotMat), Vector3.UnitY
            );
            Renderer.LoadModelView(lookAt);

            Renderer.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Renderer.Enable(EnableCap.DepthTest);
            Renderer.Disable(EnableCap.Blend);

            Model.DrawModel(lookAt * projectionMatrix, lookAt);

            ShaderManager.SetLightColorInTheSameThread(currentDayColor);
            Renderer.PopFBO();
            Renderer.PopShader();
            Renderer.BindFramebuffer(FramebufferTarget.Framebuffer, Renderer.FBOBound);
            Renderer.BindShader(Renderer.ShaderBound);
            Renderer.Disable(EnableCap.DepthTest);
            Renderer.Enable(EnableCap.Blend);
            Renderer.Viewport(0, 0, GameSettings.Width, GameSettings.Height);
            Model.PrematureCulling = meshPrematureCulling;
            return Framebuffer.TextureId[0];
        }

        public static void Update()
        {
            _rotation += Time.DeltaTime * 25f;
        }

        private static FBO Framebuffer => InventoryItemRenderer.Framebuffer;
    }
}