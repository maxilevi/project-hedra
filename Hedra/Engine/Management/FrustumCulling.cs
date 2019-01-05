/*
 * Author: Zaphyk
 * Date: 03/02/2016
 * Time: 11:42 p.m.
 *
 */

using System;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.Management
{
    public class FrustumCulling
    {
        public static float Aspect = 1.45f;
        public const float ZNear = 2.0f;
        public const float ZFar = 4096.0f;

        public const int A = 0;
        public const int B = 1;
        public const int C = 2;
        public const int D = 3;
        
        private readonly float[,] Frustum = new float[6,4];
        private readonly Vector4[] Planes = new Vector4[6];
        public readonly Vector3[] Points = new Vector3[8];
        private readonly CollisionShape FrustumShape = new CollisionShape(new Vector3[8]);
        private readonly Box _frustumBroadphase = new Box();
        private readonly Box _cacheBox = new Box();
        public Matrix4 ProjectionMatrix;
        public Matrix4 ModelViewMatrix = Matrix4.Identity;
        private bool IsFrustumCalculated;
        
        public bool IsInsideFrustum(ICullable CullableObject, float Multiplier = 1)
        {
            if (!CullableObject.Enabled) return false;
            if (CullableObject.PrematureCulling)
            {
                if ((CullableObject.Position - GameManager.Player.Position).LengthSquared 
                    > GeneralSettings.DrawDistanceSquared) 
                    return false;
            }
            var box = CullableObject.CullingBox;
            if (box == null) return false;
            _cacheBox.Min = box.Min * Multiplier + CullableObject.Position;
            _cacheBox.Max = box.Max * Multiplier + CullableObject.Position;
            return this.BoxInFrustum(_cacheBox);
        }
        
        private void NormalizePlane(float[,] Frustum , int side)
        {
            float magnitude = (float)Math.Sqrt( Frustum[side,A] * Frustum[side,A] + 
                                                Frustum[side,B] * Frustum[side,B] + 
                                                Frustum[side,C] * Frustum[side,C] );
            Frustum[side,A] /= magnitude;
            Frustum[side,B] /= magnitude;
            Frustum[side,C] /= magnitude;
            Frustum[side,D] /= magnitude; 
        }
        
        public void CalculateFrustum(Matrix4 Proj, Matrix4 Modl){
            if(GameSettings.LockFrustum) return;
       
            float[] Clip = new float[16];

            Clip[0] = (Modl.M11 * Proj.M11) + (Modl.M12 * Proj.M21) + (Modl.M13 * Proj.M31) + (Modl.M14 * Proj.M41);
            Clip[1] = (Modl.M11 * Proj.M12) + (Modl.M12 * Proj.M22) + (Modl.M13 * Proj.M32) + (Modl.M14 * Proj.M42);
            Clip[2] = (Modl.M11 * Proj.M13) + (Modl.M12 * Proj.M23) + (Modl.M13 * Proj.M33) + (Modl.M14 * Proj.M43);
            Clip[3] = (Modl.M11 * Proj.M14) + (Modl.M12 * Proj.M24) + (Modl.M13 * Proj.M34) + (Modl.M14 * Proj.M44);

            Clip[4] = (Modl.M21 * Proj.M11) + (Modl.M22 * Proj.M21) + (Modl.M23 * Proj.M31) + (Modl.M24 * Proj.M41);
            Clip[5] = (Modl.M21 * Proj.M12) + (Modl.M22 * Proj.M22) + (Modl.M23 * Proj.M32) + (Modl.M24 * Proj.M42);
            Clip[6] = (Modl.M21 * Proj.M13) + (Modl.M22 * Proj.M23) + (Modl.M23 * Proj.M33) + (Modl.M24 * Proj.M43);
            Clip[7] = (Modl.M21 * Proj.M14) + (Modl.M22 * Proj.M24) + (Modl.M23 * Proj.M34) + (Modl.M24 * Proj.M44);

            Clip[8] = (Modl.M31 * Proj.M11) + (Modl.M32 * Proj.M21) + (Modl.M33 * Proj.M31) + (Modl.M34 * Proj.M41);
            Clip[9] = (Modl.M31 * Proj.M12) + (Modl.M32 * Proj.M22) + (Modl.M33 * Proj.M32) + (Modl.M34 * Proj.M42);
            Clip[10] = (Modl.M31 * Proj.M13) + (Modl.M32 * Proj.M23) + (Modl.M33 * Proj.M33) +
                       (Modl.M34 * Proj.M43);
            Clip[11] = (Modl.M31 * Proj.M14) + (Modl.M32 * Proj.M24) + (Modl.M33 * Proj.M34) +
                       (Modl.M34 * Proj.M44);

            Clip[12] = (Modl.M41 * Proj.M11) + (Modl.M42 * Proj.M21) + (Modl.M43 * Proj.M31) +
                       (Modl.M44 * Proj.M41);
            Clip[13] = (Modl.M41 * Proj.M12) + (Modl.M42 * Proj.M22) + (Modl.M43 * Proj.M32) +
                       (Modl.M44 * Proj.M42);
            Clip[14] = (Modl.M41 * Proj.M13) + (Modl.M42 * Proj.M23) + (Modl.M43 * Proj.M33) +
                       (Modl.M44 * Proj.M43);
            Clip[15] = (Modl.M41 * Proj.M14) + (Modl.M42 * Proj.M24) + (Modl.M43 * Proj.M34) +
                       (Modl.M44 * Proj.M44);


            //Lado derecho del Frustum
            Frustum[(int) ClippingPlane.RIGHT, A] = Clip[3] - Clip[0];
            Frustum[(int) ClippingPlane.RIGHT, B] = Clip[7] - Clip[4];
            Frustum[(int) ClippingPlane.RIGHT, C] = Clip[11] - Clip[8];
            Frustum[(int) ClippingPlane.RIGHT, D] = Clip[15] - Clip[12];

            NormalizePlane(Frustum, (int) ClippingPlane.RIGHT);

            Frustum[(int) ClippingPlane.LEFT, A] = Clip[3] + Clip[0];
            Frustum[(int) ClippingPlane.LEFT, B] = Clip[7] + Clip[4];
            Frustum[(int) ClippingPlane.LEFT, C] = Clip[11] + Clip[8];
            Frustum[(int) ClippingPlane.LEFT, D] = Clip[15] + Clip[12];

            // Normalize the (int) ClippingPlane.LEFT side
            NormalizePlane(Frustum, (int) ClippingPlane.LEFT);

            // This will extract the (int) ClippingPlane.BOTTOM side of the frustum
            Frustum[(int) ClippingPlane.BOTTOM, A] = Clip[3] + Clip[1];
            Frustum[(int) ClippingPlane.BOTTOM, B] = Clip[7] + Clip[5];
            Frustum[(int) ClippingPlane.BOTTOM, C] = Clip[11] + Clip[9];
            Frustum[(int) ClippingPlane.BOTTOM, D] = Clip[15] + Clip[13];

            // Normalize the (int) ClippingPlane.BOTTOM side
            NormalizePlane(Frustum, (int) ClippingPlane.BOTTOM);

            // This will extract the (int) ClippingPlane.TOP side of the frustum
            Frustum[(int) ClippingPlane.TOP, A] = Clip[3] - Clip[1];
            Frustum[(int) ClippingPlane.TOP, B] = Clip[7] - Clip[5];
            Frustum[(int) ClippingPlane.TOP, C] = Clip[11] - Clip[9];
            Frustum[(int) ClippingPlane.TOP, D] = Clip[15] - Clip[13];

            // Normalize the (int) ClippingPlane.TOP side
            NormalizePlane(Frustum, (int) ClippingPlane.TOP);

            // This will extract the (int) ClippingPlane.BACK side of the frustum
            Frustum[(int) ClippingPlane.BACK, A] = Clip[3] - Clip[2];
            Frustum[(int) ClippingPlane.BACK, B] = Clip[7] - Clip[6];
            Frustum[(int) ClippingPlane.BACK, C] = Clip[11] - Clip[10];
            Frustum[(int) ClippingPlane.BACK, D] = Clip[15] - Clip[14];

            // Normalize the (int) ClippingPlane.BACK side
            NormalizePlane(Frustum, (int) ClippingPlane.BACK);

            // This will extract the (int) ClippingPlane.FRONT side of the frustum
            Frustum[(int) ClippingPlane.FRONT, A] = Clip[3] + Clip[2];
            Frustum[(int) ClippingPlane.FRONT, B] = Clip[7] + Clip[6];
            Frustum[(int) ClippingPlane.FRONT, C] = Clip[11] + Clip[10];
            Frustum[(int) ClippingPlane.FRONT, D] = Clip[15] + Clip[14];

            // Normalize the ClippingPlane.FRONT side
            NormalizePlane(Frustum, (int) ClippingPlane.FRONT);

            // FIXME: this is a really ugly hack, we should fix this.
            for (var i = 0; i < Planes.Length; i++)
            {
                Planes[i] = new Vector4(Frustum[i, A], Frustum[i, B], Frustum[i, C], Frustum[i, D]);
            }
            Points[0] = new Vector3(-1.0f, -1.0f, 0.0f);
            Points[1] = new Vector3(1.0f, -1.0f, 0.0f);
            Points[2] = new Vector3(-1.0f, 1.0f, 0.0f);
            Points[3] = new Vector3(1.0f, 1.0f, 0.0f);
            Points[4] = new Vector3(-1.0f, -1.0f, 1.0f);
            Points[5] = new Vector3(1.0f, -1.0f, 1.0f);
            Points[6] = new Vector3(-1.0f, 1.0f, 1.0f);
            Points[7] = new Vector3(1.0f, 1.0f, 1.0f);

            for (var i = 0; i < Points.Length; i++)
            {
                Points[i] = Vector3.TransformPosition(Points[i], Modl);
                FrustumShape.Vertices[i] = Points[i];
            }
            var center = Vector3.TransformPosition(Vector3.Zero, Modl.Inverted());
            const int size = 96;
            _frustumBroadphase.Min = center - Vector3.One * size;
            _frustumBroadphase.Max = center + Vector3.One * size;
        }
        
        public void SetFrustum(Matrix4 View)
        {
            Aspect = GameSettings.Width / (float)GameSettings.Height;
            ModelViewMatrix = View;
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(GameSettings.Fov * Mathf.Radian, Aspect, ZNear, ZFar);
            Renderer.LoadProjection(ProjectionMatrix);
            Renderer.LoadModelView(ModelViewMatrix);            
        }
        
        public void SetViewport()
        {
            SetViewport(GameSettings.Width, GameSettings.Height);
        }
        
        public void SetViewport(float Width, float Height)
        {
            SetViewport((int)Width, (int)Height);
        }

        public void SetViewport(int Width, int Height)
        {
            Renderer.Viewport(0, 0, Width, Height);
        }

        public bool VerticesInFrustum(Vector3[] Vertices)
        {
            for (var i = 0; i < Vertices.Length; i++)
            {
                if (this.PointInFrustum(Vertices[i])) return true;
            }
            return false;
        }

        public bool ShapeInFrustum(CollisionShape Shape)
        {
            return this.VerticesInFrustum(Shape.Vertices);
        }

        public bool BoxInFrustum(Box Box)
        {
            return FrustumCollidesWithBox(Box) || FrustumInBox(Box) || VerticesInFrustum(Box.Vertices);
        }

        private bool FrustumInBox(Box Box)
        {
            for (var i = 0; i < Points.Length; i++)
            {
                if (Physics.AABBvsPoint(Box, Points[i]))
                    return true;
            }
            return false;
        }

        public bool FrustumCollidesWithBox(Box Box)
        {
            return Physics.Collides(Box, _frustumBroadphase);
        }

        public bool PointInFrustum(Vector3 Point)
        {
            for (var i = 0; i < 6; i++)
            {
                if (Frustum[i, A] * Point.X + Frustum[i, B] * Point.Y + Frustum[i, C] * Point.Z + Frustum[i, D] <= 0)
                    return false;
            }
            return true;
        }
    }

    public enum ClippingPlane
    {
        RIGHT,
        LEFT,
        BOTTOM,
        TOP,
        BACK,
        FRONT
    }
}