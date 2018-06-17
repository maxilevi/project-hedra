/*
 * Author: Zaphyk
 * Date: 05/02/2016
 * Time: 09:57 p.m.
 *
 */
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;

namespace Hedra.Engine
{
	/// <summary>
	/// A math library which contains different functions
	/// which are quicker but less accurate than the originals
	/// </summary>
	public static class Mathf
	{
		public const float Radian = 0.0174533f;
		public const float Degree = 57.2958f;
			
		public static float FastSqrt(float z)
		{
		    if (z == 0) return 0;
		    FloatIntUnion u;
		    u.tmp = 0;
		    float xhalf = 0.5f * z;
		    u.f = z;
		    u.tmp = 0x5f375a86 - (u.tmp >> 1);
		    u.f = u.f * (1.5f - xhalf * u.f * u.f);
		    return u.f * z;
		}

	    public static Vector3 Sign(Vector3 Vector)
	    {
	        return new Vector3(Math.Sign(Vector.X), Math.Sign(Vector.Y), Math.Sign(Vector.Z));
	    }

        public static Vector3 Abs(Vector3 Vector)
	    {
	        return new Vector3(Math.Abs(Vector.X), Math.Abs(Vector.Y), Math.Abs(Vector.Z));
	    }

        public static float CosineInterpolate(float Y1, float Y2, float Mu)
        {
            float mu2 = (float) (1 - Math.Cos(Mu * Math.PI)) / 2;
            return Y1 * (1 - mu2) + Y2 * mu2;
        }

	    public static Vector3 CosineInterpolate(Vector3 Y1, Vector3 Y2, float Mu)
	    {
	        float mu2 = (float) (1 - Math.Cos(Mu * Math.PI)) / 2;
	        return Y1 * (1 - mu2) + Y2 * mu2;
	    }

        public static Vector3 RotationInDirection(Vector3 D){
			Matrix4 MV = Mathf.RotationAlign(-Vector3.UnitY, D);
			Vector3 Axis;
			float Angle;
			MV.ExtractRotation().ToAxisAngle(out Axis, out Angle);	
			return Axis * Angle * Mathf.Degree;
			
		}
		
		public static Matrix4 RotationAlign( Vector3 d, Vector3 z )
		{
		    Vector3 v = Mathf.CrossProduct( z, d );
		    float c = DotProduct( z, d );
		    float k = 1.0f/(1.0f+c);
		
		    return new Matrix4( v.X*v.X*k + c,     v.Y*v.X*k - v.Z,    v.Z*v.X*k + v.Y, 0,
		                   v.X*v.Y*k + v.Z,   v.Y*v.Y*k + c,      v.Z*v.Y*k - v.X, 0,
		                   v.X*v.Z*k - v.Y,   v.Y*v.Z*k + v.X,    v.Z*v.Z*k + c, 0,
		                   0,0,0,1);
		}
		
		public static float BarryCentric(Vector3 P1, Vector3 P2, Vector3 P3, Vector2 Pos) {
			float Det = (P2.Z - P3.Z) * (P1.X - P3.X) + (P3.X - P2.X) * (P1.Z - P3.Z);
			float L1 = ((P2.Z - P3.Z) * (Pos.X - P3.X) + (P3.X - P2.X) * (Pos.Y - P3.Z)) / Det;
			float L2 = ((P3.Z - P1.Z) * (Pos.X - P3.X) + (P1.X - P3.X) * (Pos.Y - P3.Z)) / Det;
			float L3 = 1.0f - L1 - L2;
			return L1 * P1.Y + L2 * P2.Y + L3 * P3.Y;
		}
		
		public static float DotProduct(Vector3 v1, Vector3 v2){
			return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
		}
		
		public static Vector3 CrossProduct(Vector3 v1, Vector3 v2){
  
		 Vector3 cross = new Vector3(v1.Y * v2.Z - v1.Z * v2.Y,
		     v1.Z * v2.X - v1.X * v2.Z,
		     v1.X * v2.Y - v1.Y * v2.X);
			
			return cross;
		}
		
				
		public static Vector2 Unpack(float inp, int prec)
		{
		    Vector2 outp = new Vector2(0,0);
		
		    outp.Y = inp % prec;
		    outp.X = (float) Math.Floor(inp / prec);
		
		    return outp / (prec - 1);
		}
		
		public static int FloorToInt(float a){
			return (int) Math.Floor(a);
		}
		
		public static float Pack(Vector2 input, int precision)
		{
		    Vector2 output = input;
		    output.X = (float) Math.Floor(output.X * (precision - 1));
		    output.Y = (float) Math.Floor(output.Y * (precision - 1));
		
		    return (output.X * precision) + output.Y;
		}
		
		public static Vector4 Lerp(Vector4 V1, Vector4 V2, float T)
        {
			return (1-T)*V1 + T*V2;
		}
		public static Vector3 Lerp(Vector3 V1, Vector3 V2, float T)
        {
			return (1-T)*V1 + T*V2;
		}
		public static Vector2 Lerp(Vector2 V1, Vector2 V2, float T)
        {
			return (1-T)*V1 + T*V2;
		}
		public static float Lerp(float F1, float F2, float T)
        {
			return (1-T)*F1 + T*F2;
		}
		
		public static float ToAbsoluteDegrees(float f){
			return ( f < 0 ) ? 360 - f  : (f > 360) ? f - 360 : f;
		}
		
		public static Vector3 FixNaN(Vector3 v){
			float x=v.X, y=v.Y, z=v.Z;
			if(float.IsNaN(v.X) || float.IsInfinity(v.X)) x = 0;
			if(float.IsNaN(v.Y) || float.IsInfinity(v.Y)) y = 0;
			if(float.IsNaN(v.Z) || float.IsInfinity(v.Z)) z = 0;
			
			return new Vector3(x,y,z);
		}
		
		public static Vector2 ScaleGUI(Vector2 TargetResolution, Vector2 Size){
			return Mathf.DivideVector(TargetResolution * Size, new Vector2(GameSettings.Width, GameSettings.Height)/*new Vector2(GameSettings.DeviceWidth, GameSettings.DeviceHeight)*/);
		}
		
		public static IntPtr ByteArrayToIntPtr(byte[] bytes){
			IntPtr unmanagedPointer = Marshal.AllocHGlobal(bytes.Length);
			Marshal.Copy(bytes, 0, unmanagedPointer, bytes.Length);
			Marshal.FreeHGlobal(unmanagedPointer);
			return unmanagedPointer;
		}
		
		public static Vector3 CalculateNormal(Vector3 v1, Vector3 v2, Vector3 v3){
			Vector3 Vector1 = new Vector3(v2.X - v1.X, v2.Y - v1.Y, v2.Z - v1.Z);
			Vector3 Vector2 = new Vector3(v3.X - v1.X, v3.Y - v1.Y, v3.Z - v1.Z);
			  
			 Vector3 cross = CrossProduct(Vector1, Vector2);
			  
			 cross.Normalize();
			  
			 return cross;
		}
		
		public static string TryParseToEnum(Type E, int N){
			if(E != null){
				return Enum.ToObject(E, N).ToString().Replace("_", string.Empty);
			}
			return N.ToString();
		}
		
		public static Vector3 Floor(Vector3 Input){
			return new Vector3((float)Math.Floor(Input.X), (float)Math.Floor(Input.Y), (float)Math.Floor(Input.Z));
		}
		
		public static Vector4 ToVector4(this Color c){
        	return new Vector4(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
        }
        
        public static Color ToColor(this Vector4 v){
			return Color.FromArgb((byte)Mathf.Clamp((int)(v.W * 255),0,255), (byte) Mathf.Clamp((int)(v.X * 255),0,255), (byte)Mathf.Clamp((int) (v.Y * 255),0,255), (byte)Mathf.Clamp((int) (v.Z * 255),0,255));
        }
		
		public static Matrix4 CreateTransformationMatrix(Vector3 Scale, Vector3 Position){
			Matrix4 TransMatrix = Matrix4.CreateTranslation(Position);
			TransMatrix *= Matrix4.CreateScale(Scale);
			return TransMatrix;
		}
		
		public static Matrix4 CreateTransformationMatrix(Vector3 Scale, Vector3 Position, Quaternion Rotation){
			Matrix4 TransMatrix = Matrix4.CreateTranslation(Position);
			TransMatrix *= Matrix4.CreateScale(Scale);
			TransMatrix *= Matrix4.CreateFromQuaternion(Rotation);
			return TransMatrix;
		}
		
		public static Matrix4 CreateTransformationMatrix(Vector2 Scale, Vector2 Position){
			return CreateTransformationMatrix(new Vector3(Scale.X, Scale.Y, 0), new Vector3(Position.X, Position.Y, 0));
		}

		public static Vector4d Mult(Matrix4 m, Vector4d v)
		{
			Vector4d result = new Vector4d();
			result.X = m.M11 * v.X + m.M12 * v.Y + m.M13 * v.Z + m.M14 * v.W;
			result.Y = m.M21 * v.X + m.M22 * v.Y + m.M23 * v.Z + m.M24 * v.W;
			result.Z = m.M31 * v.X + m.M32 * v.Y + m.M33 * v.Z + m.M34 * v.W;
			result.W = m.M41 * v.X + m.M42 * v.Y + m.M43 * v.Z + m.M44 * v.W;
			return result;
		}
		
		public static Vector2 ToNormalizedDeviceCoordinates(Vector2 Vec2) {
			return ToNormalizedDeviceCoordinates(Vec2.X, Vec2.Y);
		}
		
		public static Vector2 ToNormalizedDeviceCoordinates(float X, float Y) {
			float x = (2.0f * X) / (GameSettings.Width) - 1f;
			float y = (2.0f * Y) / (GameSettings.Height) - 1f;
			return new Vector2(x, y);
		}
		
		public static Vector2 FromNormalizedDeviceCoordinates(float X, float Y) {
			float x = (X+1f) * GameSettings.Width / 2.0f;
			float y = (Y+1f) * GameSettings.Height / 2.0f;
			return new Vector2(x,y);
		}
		
		public static Vector2 RandomVector2(Random Gen){
			return new Vector2((float) Gen.NextDouble(), (float) Gen.NextDouble());
		}
		
		public static Vector3 RandomVector3(Random Gen){
			return new Vector3( (float) Gen.NextDouble(), (float) Gen.NextDouble(), (float) Gen.NextDouble());
		}
		
		public static Vector3 RandomVector3ExceptY(Random Gen){
			return new Vector3( (float) Gen.NextDouble(), 0f, (float) Gen.NextDouble());
		}
		
		public static Quaternion RandomHorizontalQuaternion(Random Gen){
			return new Quaternion( (float) Gen.NextDouble() * Vector3.UnitY * 2f -Vector3.UnitY, (float) Gen.NextDouble() * 2f -1f);
		}
		
		public static Quaternion RandomQuaternion(Random Gen){
			return new Quaternion( (float) Gen.NextDouble() * 2f -1f, (float) Gen.NextDouble() * 2f -1f, (float) Gen.NextDouble() * 2f -1f, (float) Gen.NextDouble() * 2f -1f);
		}
		
		public static Vector4 DivideVector(Vector4 V1, Vector4 V2){
			return new Vector4(V1.X / V2.X, V1.Y / V2.Y, V1.Z / V2.Z, V1.W / V2.W);
		}
		
		public static Vector3 DivideVector(Vector3 V1, Vector3 V2){
			return new Vector3(V1.X / V2.X, V1.Y / V2.Y, V1.Z / V2.Z);
		}
		
		public static Vector2 DivideVector(Vector2 V1, Vector2 V2){
			return new Vector2(V1.X / V2.X, V1.Y / V2.Y);
		}
		
		public static Vector3[] CalculateNormals(Vector3[] vertexData, ushort[] elementData)
        {
            Vector3 b1, b2, normal;
            Vector3[] normalData = new Vector3[vertexData.Length];

            for (int i = 0; i < elementData.Length / 3; i++)
            {
                int cornerA = elementData[i * 3];
                int cornerB = elementData[i * 3 + 1];
                int cornerC = elementData[i * 3 + 2];

                b1 = vertexData[cornerB] - vertexData[cornerA];
                b2 = vertexData[cornerC] - vertexData[cornerA];

                normal = Mathf.CrossProduct(b1, b2).Normalized();

                normalData[cornerA] += normal;
                normalData[cornerB] += normal;
                normalData[cornerC] += normal;
            }

            for (int i = 0; i < normalData.Length; i++) normalData[i] = normalData[i].Normalized();

            return normalData;
        }

	    public static double Clamp(double Val, double Min, double Max)
	    {
	        return Val < Min ? Min : Val > Max ? Max : Val;
	    }

        public static float Clamp(float Val, float Min, float Max)
        {
            return Val < Min ? Min : Val > Max ? Max : Val;
        }

	    public static Vector3 Clamp(Vector3 Value, Vector3 Min, Vector3 Max)
	    {
	        return new Vector3(Clamp(Value.X, Min.X, Max.X), Clamp(Value.Y, Min.Y, Max.Y), Clamp(Value.Z, Min.Z, Max.Z));
	    }

        public static Vector3 Clamp(Vector3 Value, float min, float max){
			return new Vector3(Clamp(Value.X,min,max), Clamp(Value.Y,min,max), Clamp(Value.Z,min,max));
		}
		
		public static Vector4 Clamp(Vector4 v, float min, float max){
			return new Vector4(Clamp(v.X,min,max), Clamp(v.Y,min,max), Clamp(v.Z,min,max), Clamp(v.W,min,max));
		}
		
		public static Vector2 Clamp(Vector2 v, float min, float max){
			return new Vector2(Clamp(v.X,min,max), Clamp(v.Y,min,max));
		}
		
		public static Color ColorFromInt32(uint Data){
			return Color.FromArgb( (byte) (Data >> 24), (byte) (Data >> 16), (byte) (Data >> 8), (byte) (Data >> 0) );
		}
		
		public static uint Int32FromColor(Color Color){
			return (uint)( (Color.A << 24) | (Color.R << 16) | (Color.G << 8) | (Color.B << 0) );
		}
		
		public static double NextGaussian(this Random r, double mu = 0, double sigma = 1)
        {
            var u1 = r.NextDouble();
            var u2 = r.NextDouble();

            var rand_std_normal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                Math.Sin(2.0 * Math.PI * u2);

            var rand_normal = mu + sigma * rand_std_normal;

            return rand_normal;
        }
		
		public static float NextFloat(this Random r)
        {
			return (float) r.NextDouble() ;
        }

        public static int LevenshteinDistance(string Str1, string Str2)
	    {
	        if (string.IsNullOrEmpty(Str1) || string.IsNullOrEmpty(Str2)) return 0;

	        int lengthA = Str1.Length;
	        int lengthB = Str2.Length;
	        var distances = new int[lengthA + 1, lengthB + 1];
	        for (var i = 0; i <= lengthA; distances[i, 0] = i++) {}
	        for (var j = 0; j <= lengthB; distances[0, j] = j++) {}

	        for (var i = 1; i <= lengthA; i++)
	        for (var j = 1; j <= lengthB; j++)
	        {
	            int cost = Str2[j - 1] == Str1[i - 1] ? 0 : 1;
	            distances[i, j] = Math.Min
	            (
	                Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
	                distances[i - 1, j - 1] + cost
	            );
	        }
	        return distances[lengthA, lengthB];
	    }

        /*
		public static int Compress(this Vector3 To){
			float Factor = 21.325f;
			return Color.FromArgb(255, (byte)(To.X * Factor+128), (byte)(To.Y * Factor+128), (byte)(To.Z * Factor+128)).ToArgb();
		}
		
		public static Vector3 Decompress(this int From){
			float Factor = 21.325f;
			Color C = Color.FromArgb(From);
			return new Vector3( (C.R - 128) * Factor, (C.G - 128) * Factor, (C.B - 128) * Factor );
		}*/

        public static Color Lerp(this Color Origin, Color Target, float T){
			return Color.FromArgb( (byte) Mathf.Lerp(Origin.A, Target.A, T),
			                       (byte) Mathf.Lerp(Origin.R, Target.R, T),
			                       (byte) Mathf.Lerp(Origin.G, Target.G, T),
			                       (byte) Mathf.Lerp(Origin.B, Target.B, T));
		}

        public static Vector3 NormalizedFast(this Vector3 Point){
			Vector3 Direction = Point;
			Direction.NormalizeFast();
			return Direction;
		}
		
		public static Vector2 NormalizedFast(this Vector2 Point){
			Vector2 Direction = Point;
			Direction.NormalizeFast();
			return Direction;
		}
		
		[StructLayout(LayoutKind.Explicit)]
	    private struct FloatIntUnion
	    {
	        [FieldOffset(0)]
	        public float f;
	
	        [FieldOffset(0)]
	        public int tmp;
	    }

        /// <summary>
        /// X = 2 ^ round( log(n, 2) )
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
	    public static int ToNearestPo2(int n)
	    {
	        return (int) Math.Pow(2, (Math.Ceiling(Math.Log(n, 2))));
	    }
	}
}
