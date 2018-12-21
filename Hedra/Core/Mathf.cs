/*
 * Author: Zaphyk
 * Date: 05/02/2016
 * Time: 09:57 p.m.
 *
 */

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Game;
using OpenTK;

namespace Hedra.Core
{
    public static class Mathf
    {
        public const float Radian = 0.0174533f;
        public const float Degree = 57.2958f;
            
        public static float FastSqrt(float Z)
        {
            if (Z == 0) return 0;
            FloatIntUnion u;
            u.tmp = 0;
            var xhalf = 0.5f * Z;
            u.f = Z;
            u.tmp = 0x5f375a86 - (u.tmp >> 1);
            u.f = u.f * (1.5f - xhalf * u.f * u.f);
            return u.f * Z;
        }

        public static float CosineInterpolate(float Y1, float Y2, float Mu)
        {
            float mu2 = (float) (1 - Math.Cos(Mu * Math.PI)) / 2;
            return Y1 * (1 - mu2) + Y2 * mu2;
        }
        
        public static Matrix4 RotationAlign( Vector3 D, Vector3 Z )
        {
            Vector3 v = Vector3.Cross( Z, D );
            float c = Vector3.Dot( Z, D );
            float k = 1.0f/(1.0f+c);
        
            return new Matrix4( 
                v.X*v.X*k + c,     v.Y*v.X*k - v.Z,    v.Z*v.X*k + v.Y, 0,
                v.X*v.Y*k + v.Z,   v.Y*v.Y*k + c,      v.Z*v.Y*k - v.X, 0,
                v.X*v.Z*k - v.Y,   v.Y*v.Z*k + v.X,    v.Z*v.Z*k + c,   0,
                0,                 0,                  0,               1
            );
        }
        
        public static float BarryCentric(Vector3 P1, Vector3 P2, Vector3 P3, Vector2 Pos)
        {
            float det = (P2.Z - P3.Z) * (P1.X - P3.X) + (P3.X - P2.X) * (P1.Z - P3.Z);
            float l1 = ((P2.Z - P3.Z) * (Pos.X - P3.X) + (P3.X - P2.X) * (Pos.Y - P3.Z)) / det;
            float l2 = ((P3.Z - P1.Z) * (Pos.X - P3.X) + (P1.X - P3.X) * (Pos.Y - P3.Z)) / det;
            float l3 = 1.0f - l1 - l2;
            return l1 * P1.Y + l2 * P2.Y + l3 * P3.Y;
        }
                
        public static Vector2 Unpack(float Inp, int Prec)
        {
            Vector2 outp = new Vector2(0,0);
        
            outp.Y = Inp % Prec;
            outp.X = (float) Math.Floor(Inp / Prec);
        
            return outp / (Prec - 1);
        }
        
        public static int FloorToInt(float A)
        {
            return (int) Math.Floor(A);
        }
        
        public static float Pack(Vector2 Input, int Precision)
        {
            Vector2 output = Input;
            output.X = (float) Math.Floor(output.X * (Precision - 1));
            output.Y = (float) Math.Floor(output.Y * (Precision - 1));
        
            return (output.X * Precision) + output.Y;
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

        public static Vector2 ScaleGui(Vector2 TargetResolution, Vector2 Size)
        {
            return Mathf.DivideVector(TargetResolution * Size, new Vector2(GameSettings.Width, GameSettings.Height)/*new Vector2(GameSettings.DeviceWidth, GameSettings.DeviceHeight)*/);
        }
        
        public static Vector3 CalculateNormal(Vector3 V1, Vector3 V2, Vector3 V3)
        {
            Vector3 vector1 = new Vector3(V2.X - V1.X, V2.Y - V1.Y, V2.Z - V1.Z);
            Vector3 vector2 = new Vector3(V3.X - V1.X, V3.Y - V1.Y, V3.Z - V1.Z);
              
             Vector3 cross = Vector3.Cross(vector1, vector2);
              
             cross.Normalize();
              
             return cross;
        }

        public static Vector4 ToVector4(this Color C)
        {
            return new Vector4(C.R / 255f, C.G / 255f, C.B / 255f, C.A / 255f);
        }
        
        public static Color ToColor(this Vector4 V)
        {
            return Color.FromArgb((byte)Mathf.Clamp((int)(V.W * 255),0,255), (byte) Mathf.Clamp((int)(V.X * 255),0,255), (byte)Mathf.Clamp((int) (V.Y * 255),0,255), (byte)Mathf.Clamp((int) (V.Z * 255),0,255));
        }

        public static Vector2 ToNormalizedDeviceCoordinates(Vector2 Vec2)
        {
            return ToNormalizedDeviceCoordinates(Vec2.X, Vec2.Y);
        }
        
        public static Vector2 ToNormalizedDeviceCoordinates(float X, float Y)
        {
            float x = (2.0f * X) / (GameSettings.Width) - 1f;
            float y = (2.0f * Y) / (GameSettings.Height) - 1f;
            return new Vector2(x, y);
        }

        public static Vector2 ToNormalizedDeviceCoordinates(Vector2 Position, Vector2 Surface)
        {
            float x = (2.0f * Position.X) / (Surface.X) - 1f;
            float y = (2.0f * Position.Y) / (Surface.Y) - 1f;
            return new Vector2(x, y);
        }

        public static Vector2 FromNormalizedDeviceCoordinates(float X, float Y)
        {
            float x = (X+1f) * GameSettings.Width / 2.0f;
            float y = (Y+1f) * GameSettings.Height / 2.0f;
            return new Vector2(x,y);
        }

        public static Vector3 RandomVector3(Random Gen)
        {
            return new Vector3( (float) Gen.NextDouble(), (float) Gen.NextDouble(), (float) Gen.NextDouble());
        }
        
        public static Vector3 DivideVector(Vector3 V1, Vector3 V2){
            return new Vector3(V1.X / V2.X, V1.Y / V2.Y, V1.Z / V2.Z);
        }
        
        public static Vector2 DivideVector(Vector2 V1, Vector2 V2){
            return new Vector2(V1.X / V2.X, V1.Y / V2.Y);
        }

        public static double Clamp(double Val, double Min, double Max)
        {
            return Math.Max(Math.Min(Val, Max), Min);
        }

        public static float Clamp(float Val, float Min, float Max)
        {
            return Math.Max(Math.Min(Val, Max), Min);
        }

        public static Vector3 Clamp(Vector3 Value, Vector3 Min, Vector3 Max)
        {
            return new Vector3(Clamp(Value.X, Min.X, Max.X), Clamp(Value.Y, Min.Y, Max.Y), Clamp(Value.Z, Min.Z, Max.Z));
        }

        public static Vector3 Clamp(Vector3 Value, float Min, float Max){
            return new Vector3(Clamp(Value.X,Min,Max), Clamp(Value.Y,Min,Max), Clamp(Value.Z,Min,Max));
        }
        
        public static Vector4 Clamp(Vector4 V, float Min, float Max)
        {
            return new Vector4(Clamp(V.X,Min,Max), Clamp(V.Y,Min,Max), Clamp(V.Z,Min,Max), Clamp(V.W,Min,Max));
        }
        
        public static Vector2 Clamp(Vector2 V, float Min, float Max)
        {
            return new Vector2(Clamp(V.X,Min,Max), Clamp(V.Y,Min,Max));
        }
        
        public static Color ColorFromInt32(uint Data)
        {
            return Color.FromArgb( (byte) (Data >> 24), (byte) (Data >> 16), (byte) (Data >> 8), (byte) (Data >> 0) );
        }
        
        public static uint Int32FromColor(Color Color)
        {
            return (uint)( (Color.A << 24) | (Color.R << 16) | (Color.G << 8) | (Color.B << 0) );
        }
        
        public static float NextFloat(this IRandom Random)
        {
            return (float) Random.NextDouble();
        }
        
        public static float NextFloat(this Random Random)
        {
            return (float) Random.NextDouble();
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

        public static Color Lerp(this Color Origin, Color Target, float T)
        {
            return Color.FromArgb( (byte) Lerp(Origin.A, Target.A, T),
                                   (byte) Lerp(Origin.R, Target.R, T),
                                   (byte) Lerp(Origin.G, Target.G, T),
                                   (byte) Lerp(Origin.B, Target.B, T));
        }

        public static Vector3 NormalizedFast(this Vector3 Point)
        {
            var direction = Point;
            direction.NormalizeFast();
            return direction;
        }
        
        public static Vector2 NormalizedFast(this Vector2 Point)
        {
            var direction = Point;
            direction.NormalizeFast();
            return direction;
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
        /// <param name="N"></param>
        /// <returns></returns>
        public static int ToNearestPo2(int N)
        {
            return (int) Math.Pow(2, (Math.Ceiling(Math.Log(N, 2))));
        }
    }
}