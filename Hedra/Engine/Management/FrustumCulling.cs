/*
 * Author: Zaphyk
 * Date: 03/02/2016
 * Time: 11:42 p.m.
 *
 */
using System;
using OpenTK;
using Hedra.Engine.Scenes;
using Hedra.Engine.Management;
using OpenTK.Graphics.OpenGL;

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
        public Matrix4 ProjectionMatrix;
        public Matrix4 ModelViewMatrix = Matrix4.Identity;
        private bool IsFrustumCalculated;
		
		public bool IsInsideFrustum(IRenderable RenderableObject){
			if(RenderableObject is ICullable){
        		ICullable CullableObject = (ICullable) RenderableObject;
        		if(CullableObject.Enabled){
        			if(!CullableObject.DontCull){
		        		if(CullableObject.Shape == RenderShape.CUBE)
		        			return CubeInFrustum(CullableObject.Position.X, CullableObject.Position.Y, CullableObject.Position.Z, CullableObject.Size);
        			}
        			return true;
        		}
        		return false;
			}
			return true;
		}
        public bool IsInsideFrustum(ICullable CullableObject){
            if (!CullableObject.Enabled) return false;
            if (CullableObject.DontCull) return true;

            switch (CullableObject.Shape)
            {
                case RenderShape.CUBE:
                    return this.CubeInFrustum(CullableObject.Position.X, CullableObject.Position.Y, CullableObject.Position.Z, CullableObject.Size);
                case RenderShape.POINT:
                    return this.PointInFrustum(CullableObject.Position.X, CullableObject.Position.Y, CullableObject.Position.Z);
                case RenderShape.SPHERE:
                    return this.PointInFrustum(CullableObject.Position.X, CullableObject.Position.Y, CullableObject.Position.Z);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void NormalizePlane(float[,] Frustum , int side){
			float magnitude = (float)Math.Sqrt( Frustum[side,A] * Frustum[side,A] + 
										   		Frustum[side,B] * Frustum[side,B] + 
										   		Frustum[side,C] * Frustum[side,C] );
			Frustum[side,A] /= magnitude;
			Frustum[side,B] /= magnitude;
			Frustum[side,C] /= magnitude;
			Frustum[side,D] /= magnitude; 
		}
		
        public void CalculateFrustum(Matrix4 Proj, Matrix4 Modl){

        	float[] Clip = new float[16];

			Clip[ 0 ] = ( Modl.M11 * Proj.M11 ) + ( Modl.M12 * Proj.M21 ) + ( Modl.M13 * Proj.M31 ) + ( Modl.M14 * Proj.M41 );
            Clip[ 1 ] = ( Modl.M11 * Proj.M12 ) + ( Modl.M12 * Proj.M22 ) + ( Modl.M13 * Proj.M32 ) + ( Modl.M14 * Proj.M42 );
            Clip[ 2 ] = ( Modl.M11 * Proj.M13 ) + ( Modl.M12 * Proj.M23 ) + ( Modl.M13 * Proj.M33 ) + ( Modl.M14 * Proj.M43 );
            Clip[ 3 ] = ( Modl.M11 * Proj.M14 ) + ( Modl.M12 * Proj.M24 ) + ( Modl.M13 * Proj.M34 ) + ( Modl.M14 * Proj.M44 );

            Clip[ 4 ] = ( Modl.M21 * Proj.M11 ) + ( Modl.M22 * Proj.M21 ) + ( Modl.M23 * Proj.M31 ) + ( Modl.M24 * Proj.M41 );
            Clip[ 5 ] = ( Modl.M21 * Proj.M12 ) + ( Modl.M22 * Proj.M22 ) + ( Modl.M23 * Proj.M32 ) + ( Modl.M24 * Proj.M42 );
            Clip[ 6 ] = ( Modl.M21 * Proj.M13 ) + ( Modl.M22 * Proj.M23 ) + ( Modl.M23 * Proj.M33 ) + ( Modl.M24 * Proj.M43 );
            Clip[ 7 ] = ( Modl.M21 * Proj.M14 ) + ( Modl.M22 * Proj.M24 ) + ( Modl.M23 * Proj.M34 ) + ( Modl.M24 * Proj.M44 );

            Clip[ 8 ] = ( Modl.M31 * Proj.M11 ) + ( Modl.M32 * Proj.M21 ) + ( Modl.M33 * Proj.M31 ) + ( Modl.M34 * Proj.M41 );
            Clip[ 9 ] = ( Modl.M31 * Proj.M12 ) + ( Modl.M32 * Proj.M22 ) + ( Modl.M33 * Proj.M32 ) + ( Modl.M34 * Proj.M42 );
            Clip[ 10 ] = ( Modl.M31 * Proj.M13 ) + ( Modl.M32 * Proj.M23 ) + ( Modl.M33 * Proj.M33 ) + ( Modl.M34 * Proj.M43 );
            Clip[ 11 ] = ( Modl.M31 * Proj.M14 ) + ( Modl.M32 * Proj.M24 ) + ( Modl.M33 * Proj.M34 ) + ( Modl.M34 * Proj.M44 );

            Clip[ 12 ] = ( Modl.M41 * Proj.M11 ) + ( Modl.M42 * Proj.M21 ) + ( Modl.M43 * Proj.M31 ) + ( Modl.M44 * Proj.M41 );
            Clip[ 13 ] = ( Modl.M41 * Proj.M12 ) + ( Modl.M42 * Proj.M22 ) + ( Modl.M43 * Proj.M32 ) + ( Modl.M44 * Proj.M42 );
            Clip[ 14 ] = ( Modl.M41 * Proj.M13 ) + ( Modl.M42 * Proj.M23 ) + ( Modl.M43 * Proj.M33 ) + ( Modl.M44 * Proj.M43 );
            Clip[ 15 ] = ( Modl.M41 * Proj.M14 ) + ( Modl.M42 * Proj.M24 ) + ( Modl.M43 * Proj.M34 ) + ( Modl.M44 * Proj.M44 );
			
		
			//Lado derecho del Frustum
			Frustum[(int) ClippingPlane.RIGHT , A] = Clip[ 3] - Clip[ 0];
			Frustum[(int) ClippingPlane.RIGHT , B] = Clip[ 7] - Clip[ 4];
			Frustum[(int) ClippingPlane.RIGHT , C] = Clip[11] - Clip[ 8];
			Frustum[(int) ClippingPlane.RIGHT , D] = Clip[15] - Clip[12];
		
			NormalizePlane(Frustum, (int) ClippingPlane.RIGHT);

			Frustum[(int) ClippingPlane.LEFT , A] = Clip[ 3] + Clip[ 0];
			Frustum[(int) ClippingPlane.LEFT , B] = Clip[ 7] + Clip[ 4];
			Frustum[(int) ClippingPlane.LEFT , C] = Clip[11] + Clip[ 8];
			Frustum[(int) ClippingPlane.LEFT , D] = Clip[15] + Clip[12];
		
			// Normalize the (int) ClippingPlane.LEFT side
			NormalizePlane(Frustum, (int) ClippingPlane.LEFT);
		
			// This will extract the (int) ClippingPlane.BOTTOM side of the frustum
			Frustum[(int) ClippingPlane.BOTTOM , A] = Clip[ 3] + Clip[ 1];
			Frustum[(int) ClippingPlane.BOTTOM , B] = Clip[ 7] + Clip[ 5];
			Frustum[(int) ClippingPlane.BOTTOM , C] = Clip[11] + Clip[ 9];
			Frustum[(int) ClippingPlane.BOTTOM , D] = Clip[15] + Clip[13];
		
			// Normalize the (int) ClippingPlane.BOTTOM side
			NormalizePlane(Frustum, (int) ClippingPlane.BOTTOM);
		
			// This will extract the (int) ClippingPlane.TOP side of the frustum
			Frustum[(int) ClippingPlane.TOP , A] = Clip[ 3] - Clip[ 1];
			Frustum[(int) ClippingPlane.TOP , B] = Clip[ 7] - Clip[ 5];
			Frustum[(int) ClippingPlane.TOP , C] = Clip[11] - Clip[ 9];
			Frustum[(int) ClippingPlane.TOP , D] = Clip[15] - Clip[13];
		
			// Normalize the (int) ClippingPlane.TOP side
			NormalizePlane(Frustum, (int) ClippingPlane.TOP);
	
			// This will extract the (int) ClippingPlane.BACK side of the frustum
			Frustum[(int) ClippingPlane.BACK , A] = Clip[ 3] - Clip[ 2];
			Frustum[(int) ClippingPlane.BACK , B] = Clip[ 7] - Clip[ 6];
			Frustum[(int) ClippingPlane.BACK , C] = Clip[11] - Clip[10];
			Frustum[(int) ClippingPlane.BACK , D] = Clip[15] - Clip[14];
		
			// Normalize the (int) ClippingPlane.BACK side
			NormalizePlane(Frustum, (int) ClippingPlane.BACK);
		
			// This will extract the (int) ClippingPlane.FRONT side of the frustum
			Frustum[(int) ClippingPlane.FRONT , A] = Clip[ 3] + Clip[ 2];
			Frustum[(int) ClippingPlane.FRONT , B] = Clip[ 7] + Clip[ 6];
			Frustum[(int) ClippingPlane.FRONT , C] = Clip[11] + Clip[10];
			Frustum[(int) ClippingPlane.FRONT , D] = Clip[15] + Clip[14];
		
			// Normalize the ClippingPlane.FRONT side
			NormalizePlane(Frustum, (int) ClippingPlane.FRONT);
		}
        
		public void SetFrustum(Matrix4 View)
		{
		    Aspect = 1.45f / (float) GameSettings.Height * (float) GameSettings.DeviceHeight;
        	GL.MatrixMode(MatrixMode.Projection);

	       	ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(GameSettings.Fov * Mathf.Radian, Aspect, ZNear, ZFar);
	        GL.LoadMatrix(ref ProjectionMatrix);
	            
	        GL.MatrixMode(MatrixMode.Modelview);
	        ModelViewMatrix = View;
			GL.LoadMatrix(ref ModelViewMatrix);
			
		}

	    public void SetViewport()
	    {
	        GL.Viewport(0,0, GameSettings.Width, GameSettings.Height);
        }
        
         public bool CubeInFrustum( float x , float y , float z , Vector3 size){
            for( int i = 0; i < 6; i++ )
            {
                if( Frustum[ i , A ] * ( x - size.X ) + Frustum[ i , B ] * ( y - size.Y ) + Frustum[ i , C ] * ( z - size.Z ) + Frustum[ i , D ] > 0 )
                    continue;
                if( Frustum[ i , A ] * ( x + size.X ) + Frustum[ i , B ] * ( y - size.Y ) + Frustum[ i , C ] * ( z - size.Z ) + Frustum[ i , D ] > 0 )
                    continue;
                if( Frustum[ i , A ] * ( x - size.X ) + Frustum[ i , B ] * ( y + size.Y ) + Frustum[ i , C ] * ( z - size.Z ) + Frustum[ i , D ] > 0 )
                    continue;
                if( Frustum[ i , A ] * ( x + size.X ) + Frustum[ i , B ] * ( y + size.Y ) + Frustum[ i , C ] * ( z - size.Z ) + Frustum[ i , D ] > 0 )
                    continue;
                if( Frustum[ i , A ] * ( x - size.X ) + Frustum[ i , B ] * ( y - size.Y ) + Frustum[ i , C ] * ( z + size.Z ) + Frustum[ i , D ] > 0 )
                    continue;
                if( Frustum[ i , A ] * ( x + size.X ) + Frustum[ i , B ] * ( y - size.Y ) + Frustum[ i , C ] * ( z + size.Z ) + Frustum[ i , D ] > 0 )
                    continue;
                if( Frustum[ i , A ] * ( x - size.X ) + Frustum[ i , B ] * ( y + size.Y ) + Frustum[ i , C ] * ( z + size.Z ) + Frustum[ i , D ] > 0 )
                    continue;
                if( Frustum[ i , A ] * ( x + size.X ) + Frustum[ i , B ] * ( y + size.Y ) + Frustum[ i , C ] * ( z + size.Z ) + Frustum[ i , D ] > 0 )
                    continue;
                return false;
            }
            return true;
        }
        
        public bool PointInFrustum( float x, float y, float z )
		{
			for(int i = 0; i < 6; i++ )
			{
				// Calculate the plane equation and check if the point is behind a side of the frustum
				if(Frustum[i, A] * x + Frustum[i, B] * y + Frustum[i, C] * z + Frustum[i, D] <= 0)
				{
					// The point was behind a side, so it ISN'T in the frustum
					return false;
				}
			}
		
			// The point was inside of the frustum (In front of ALL the sides of the frustum)
			return true;
		}
        
        public bool PointInFrustum( Vector3 Point )
		{
        	return PointInFrustum(Point.X, Point.Y, Point.Z);
		}
	}
}
public enum ClippingPlane{
	RIGHT,
	LEFT,
	BOTTOM,
	TOP,
	BACK,
	FRONT
}
