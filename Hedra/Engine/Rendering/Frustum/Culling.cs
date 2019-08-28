using System.Linq;
using Hedra.Core;
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
        private static readonly Vector4[] UnitCube = new Vector4[8]
        {
            new Vector4(-1, -1, -1, 1),
            new Vector4(-1, -1, 1, 1),
            new Vector4(-1, 1, -1, 1),
            new Vector4(1, -1, -1, 1),
            new Vector4(1, -1, 1, 1),
            new Vector4(-1, 1, 1, 1),
            new Vector4(1, 1, -1, 1),
            new Vector4(1, 1, 1, 1),
        };
        
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

        private static Matrix4 _matrix4;
        private static bool _wasLocked;
        private static void UpdateFrustum()
        {
            Renderer.LoadModelView(ModelViewMatrix);
            Renderer.LoadProjection(ProjectionMatrix);

            if (!_wasLocked && GameSettings.LockFrustum)
            {
                _matrix4 = Player.LocalPlayer.Instance.View.ModelViewMatrix.Inverted();
                Frustum.SetMatrices(
                    ProjectionMatrix,
                    Player.LocalPlayer.Instance.View.ModelViewMatrix
                );

            }
            _wasLocked = GameSettings.LockFrustum;
            
            if (!GameSettings.LockFrustum) Frustum.SetMatrices(ProjectionMatrix, ModelViewMatrix);
        }

        public static void Draw()
        {
            if (GameSettings.DebugFrustum)
            {
                var invProjection = (Matrix4.CreatePerspectiveFieldOfView(50 * Mathf.Radian, 1.33f, ZNear, 64)).Inverted();
                var position = Vector3.Zero;
                var newCorners = UnitCube.Select(V => Vector4.Transform(V, invProjection)).Select(P => (P.Xyz / P.W) + position).Select(P => Vector3.TransformPosition(P, _matrix4)).ToArray();
                for (var i = 0; i < newCorners.Length; ++i)
                {
                    BasicGeometry.DrawPoint(newCorners[i], Vector4.One, 10);
                }
                /*
                var topPlanePosition = (newCorners[5] + newCorners[6] + newCorners[7]) / 3;
                var topPlane = new Plane(newCorners[5], newCorners[6], newCorners[7]);
                BasicGeometry.DrawLine(topPlanePosition, topPlanePosition + topPlane.Normal * 8, Vector4.One, 5);
                
                var rightPlanePosition = (newCorners[3] + newCorners[4] + newCorners[7]) / 3;
                var rightPlane = new Plane(newCorners[3], newCorners[4], newCorners[7]);
                BasicGeometry.DrawLine(rightPlanePosition, rightPlanePosition + rightPlane.Normal * 8, Vector4.One, 5);
                
                var leftPlanePosition = (newCorners[0] + newCorners[1] + newCorners[5]) / 3;
                var leftPlane = new Plane(newCorners[0], newCorners[1], newCorners[5]);
                BasicGeometry.DrawLine(leftPlanePosition, leftPlanePosition + leftPlane.Normal * 8, Vector4.One, 5);
                
                var backPlanePosition = (newCorners[0] + newCorners[2] + newCorners[3]) / 3;
                var backPlane = new Plane(-newCorners[0], newCorners[3], newCorners[2]);
                BasicGeometry.DrawLine(backPlanePosition, backPlanePosition + backPlane.Normal * 8, Vector4.One, 5);
                
                var frontPlanePosition = (newCorners[1] + newCorners[4] + newCorners[7]) / 3;
                var frontPlane = new Plane(newCorners[1], newCorners[7], newCorners[4]);
                BasicGeometry.DrawLine(frontPlanePosition, frontPlanePosition + frontPlane.Normal * 8, Vector4.One, 5);
                
                var bottomPlanePosition = (newCorners[1] + newCorners[4] + newCorners[6]) / 3;
                var bottomPlane = new Plane(newCorners[1], newCorners[4], newCorners[6]);
                BasicGeometry.DrawLine(bottomPlanePosition, bottomPlanePosition + bottomPlane.Normal * 8, Vector4.One, 5);*/
            }
        }
    }
}