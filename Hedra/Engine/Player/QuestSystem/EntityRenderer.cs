using Hedra.Core;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Game;
using Hedra.Engine.Rendering.Core;
using Hedra.Game;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.Player.QuestSystem
{
    public static class EntityRenderer
    {
        private static float _rotation;
        
        public static uint Draw(AnimatedModel Model)
        {
            var previousBound = Renderer.ShaderBound;
            var previousFBO = Renderer.FBOBound;
            Framebuffer.Bind();

            var meshPrematureCulling = Model.PrematureCulling;
            Model.PrematureCulling = false;

            var currentDayColor = ShaderManager.LightColor;
            ShaderManager.SetLightColorInTheSameThread(Vector3.One);

            var previousShadows = GameSettings.GlobalShadows;
            GameSettings.GlobalShadows = false;
            
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
            GameSettings.GlobalShadows = previousShadows;
            Renderer.BindFramebuffer(FramebufferTarget.Framebuffer, previousFBO);
            Renderer.BindShader(previousBound);
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