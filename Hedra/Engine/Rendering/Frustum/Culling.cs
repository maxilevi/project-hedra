using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using OpenTK;

namespace Hedra.Engine.Rendering.Frustum
{
    public static class Culling
    {
        private const float ZNear = 2.0f;
        private const float ZFar = 4096.0f;
        public static Matrix4 ProjectionMatrix;
        public static Matrix4 ModelViewMatrix = Matrix4.Identity;
        
        public static BoundingFrustum Frustum { get; } = new BoundingFrustum();

        public static bool IsInside(ICullable CullableObject)
        {
            CullableObject.WasCulled = true;
            if (!CullableObject.Enabled) return false;
            if (CullableObject.PrematureCulling)
            {
                if ((CullableObject.Position - GameManager.Player.Position).LengthSquared 
                    > GeneralSettings.DrawDistanceSquared) 
                    return false;
            }
            var min = CullableObject.Min + CullableObject.Position;
            var max = CullableObject.Max + CullableObject.Position;
            var isContained = Frustum.Contains(ref min, ref max);
            CullableObject.WasCulled = !isContained;
            return isContained;
        }
        
        public static bool IsInside(Vector3 Point)
        {
            return Frustum.Contains(ref Point);
        }
        
        public static void SetViewport()
        {
            SetViewport(GameSettings.Width, GameSettings.Height);
        }

        public static void SetViewport(int Width, int Height)
        {
            Renderer.Viewport(0, 0, Width, Height);
        }

        public static void BuildFrustum(Matrix4 Proj, Matrix4 View)
        {
            BuildFrustum(View);
            ProjectionMatrix = Proj;
            UpdateFrustum();
        }
        
        public static void BuildFrustum(Matrix4 View)
        {
            var aspect = GameSettings.Width / (float)GameSettings.Height;
            ModelViewMatrix = View;
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(GameSettings.Fov * Mathf.Radian, aspect, ZNear, ZFar);
            UpdateFrustum();
        }

        private static void UpdateFrustum()
        {
            Renderer.LoadModelView(ModelViewMatrix);
            Renderer.LoadProjection(ProjectionMatrix);
            if (!GameSettings.LockFrustum) Frustum.Matrix = ModelViewMatrix * ProjectionMatrix;
        }
    }
}