using Hedra.Core;
using Hedra.Engine.Bullet;
using Hedra.Engine.Game;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering.Core;
using Hedra.Game;
using OpenTK;

namespace Hedra.Engine.Rendering.Frustum
{
    public static class Culling
    {
        private const float ZNear = 2.0f;
        private const float ZFar = 4096.0f;
        public static Matrix4 ProjectionMatrix;
        public static Matrix4 ModelViewMatrix = Matrix4.Identity;

        private static BulletFrustum Frustum { get; } = new BulletFrustum();

        public static bool IsInside(ICullable CullableObject)
        {
            CullableObject.WasCulled = true;
            var isContained = Frustum.Contains(CullableObject);
            CullableObject.WasCulled = !isContained;
            return isContained;
        }

        public static void Update()
        {
            Frustum.Update();
        }

        public static void Draw()
        {
            Frustum.Draw();
        }

        public static void Add(ICullable Cullable)
        {
            Frustum.Add(Cullable);
        }

        public static void Remove(ICullable Cullable)
        {
            Frustum.Remove(Cullable);
        }

        public static void UpdateFrustumMatrices(Matrix4 ModelView, Matrix4 Projection)
        {
            Frustum.UpdateMatrices(ModelView, Projection);
        }

        public static void SetViewport()
        {
            Renderer.Viewport(0, 0, GameSettings.Width, GameSettings.Height);
        }

        public static void BuildFrustum(Matrix4 Proj, Matrix4 View)
        {
            BuildFrustum(View, ShouldUpdateFrustum: false);
            ProjectionMatrix = Proj;
            UpdateFrustum();
        }
        
        public static void BuildFrustum(Matrix4 View, bool ShouldUpdateFrustum = true)
        {
            var aspect = GameSettings.Width / (float)GameSettings.Height;
            ModelViewMatrix = View;
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(GameSettings.Fov * Mathf.Radian, aspect, ZNear, ZFar);
            if(ShouldUpdateFrustum)
                UpdateFrustum();
        }

        private static void UpdateFrustum()
        {
            Renderer.LoadModelView(ModelViewMatrix);
            Renderer.LoadProjection(ProjectionMatrix);
            if (!GameSettings.LockFrustum) Frustum.UpdateMatrices(ModelViewMatrix, ProjectionMatrix);
        }
    }
}