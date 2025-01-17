using System.Numerics;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Core;
using Hedra.Game;
using Hedra.Numerics;

namespace Hedra.Engine.Rendering.Frustum
{
    public static class Culling
    {
        private const float ZNear = 2.0f;
        private const float ZFar = 2048.0f;
        public static Matrix4x4 ProjectionMatrix;
        public static Matrix4x4 ModelViewMatrix = Matrix4x4.Identity;

        private static readonly Vector4[] UnitCube = new Vector4[8]
        {
            new Vector4(-1, -1, -1, 1),
            new Vector4(-1, -1, 1, 1),
            new Vector4(-1, 1, -1, 1),
            new Vector4(1, -1, -1, 1),
            new Vector4(1, -1, 1, 1),
            new Vector4(-1, 1, 1, 1),
            new Vector4(1, 1, -1, 1),
            new Vector4(1, 1, 1, 1)
        };

        private static Matrix4x4 _matrix4;
        private static bool _wasLocked;

        public static BoundingFrustum Frustum { get; } = new BoundingFrustum();

        public static bool IsInside(ICullable CullableObject)
        {
            CullableObject.WasCulled = true;
            if (!CullableObject.Enabled) return false;
            if (CullableObject.PrematureCulling)
                if ((CullableObject.Position - GameManager.Player.Position).LengthSquared()
                    > GeneralSettings.DrawDistanceSquared)
                    return false;
            var min = CullableObject.Min + CullableObject.Position;
            var max = CullableObject.Max + CullableObject.Position;
            var isContained = Frustum.Contains(ref min, ref max);
            CullableObject.WasCulled = !isContained;
            return isContained;
        }

        public static void SetViewport()
        {
            SetViewport(GameSettings.Width, GameSettings.Height);
        }

        public static void SetViewport(int Width, int Height)
        {
            Renderer.Viewport(0, 0, Width, Height);
        }

        public static void BuildFrustum(Matrix4x4 Proj, Matrix4x4 View)
        {
            BuildFrustum(View);
            ProjectionMatrix = Proj;
            UpdateFrustum();
        }

        public static void BuildFrustum(Matrix4x4 View)
        {
            var aspect = GameSettings.Width / (float)GameSettings.Height;
            ModelViewMatrix = View;
            ProjectionMatrix =
                Matrix4x4.CreatePerspectiveFieldOfView(GameSettings.FieldOfView * Mathf.Radian, aspect, ZNear, ZFar);
            UpdateFrustum();
        }

        private static void UpdateFrustum()
        {
            Renderer.LoadModelView(ModelViewMatrix);
            Renderer.LoadProjection(ProjectionMatrix);

            if (!_wasLocked && GameSettings.LockFrustum)
            {
                _matrix4 = LocalPlayer.Instance.View.ModelViewMatrix.Inverted();
                Frustum.SetMatrices(
                    ProjectionMatrix,
                    LocalPlayer.Instance.View.ModelViewMatrix
                );
            }

            _wasLocked = GameSettings.LockFrustum;

            if (!GameSettings.LockFrustum) Frustum.SetMatrices(ProjectionMatrix, ModelViewMatrix);
        }

        public static void Draw()
        {
            Frustum.Draw(_matrix4);
        }
    }
}