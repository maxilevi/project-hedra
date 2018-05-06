/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 25/05/2016
 * Time: 11:13 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Hedra.Engine.ComplexMath;
using Newtonsoft.Json.Converters;
using OpenTK;

namespace Hedra.Engine
{
	/// <summary>
	/// Description of Extensions.
	/// </summary>
	public static class Extensions
	{

	    public static Vector2 ScaleUI(this Vector2 Vector, Vector2 Resolution)
	    {
	        return Mathf.ScaleGUI(Resolution, Vector);
	    }

        public static string ToPascalString(this string S)
	    {
	        return Regex.Replace(S, "([A-Z])", " $1").Trim();
        }

	    public static void CopyTo(this Stream input, Stream output)
	    {
	        byte[] buffer = new byte[16 * 1024];
	        int bytesRead;
	        while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
	        {
	            output.Write(buffer, 0, bytesRead);
	        }
	    }
		
		public static Vector3 ToEuler(this Quaternion Quaternion)
		{
		    return QuaternionMath.ToEuler(Quaternion);
		}
		
		public static List<T> DeepClone<T>(this List<T> List){
			
			if(List.Count == 0) return new List<T>();	
			
			if(List[0] is ICloneable){
				var NewList = new List<T>();
				for(int i = 0; i < List.Count; i++){
					NewList.Add( (T) (List[i] as ICloneable).Clone() );
				}
				return NewList;
			}
		    return new List<T>(List);
		}
		
		public static Quaternion FromMatrixExt(Matrix4 matrix) {
			float w, x, y, z;
			float diagonal = matrix.M11 + matrix.M22 + matrix.M33;
			if (diagonal > 0) {
				float w4 = (float) (Math.Sqrt(diagonal + 1f) * 2f);
				w = w4 / 4f;
				x = (matrix.M32 - matrix.M23) / w4;
				y = (matrix.M13 - matrix.M31) / w4;
				z = (matrix.M21 - matrix.M12) / w4;
			} else if ((matrix.M11 > matrix.M22) && (matrix.M11 > matrix.M33)) {
				float x4 = (float) (Math.Sqrt(1f + matrix.M11 - matrix.M22 - matrix.M33) * 2f);
				w = (matrix.M32 - matrix.M23) / x4;
				x = x4 / 4f;
				y = (matrix.M12 + matrix.M21) / x4;
				z = (matrix.M13 + matrix.M31) / x4;
			} else if (matrix.M22 > matrix.M33) {
				float y4 = (float) (Math.Sqrt(1f + matrix.M22 - matrix.M11 - matrix.M33) * 2f);
				w = (matrix.M13 - matrix.M31) / y4;
				x = (matrix.M12 + matrix.M21) / y4;
				y = y4 / 4f;
				z = (matrix.M23 + matrix.M32) / y4;
			} else {
				float z4 = (float) (Math.Sqrt(1f + matrix.M33 - matrix.M11 - matrix.M22) * 2f);
				w = (matrix.M21 - matrix.M12) / z4;
				x = (matrix.M13 + matrix.M31) / z4;
				y = (matrix.M23 + matrix.M32) / z4;
				z = z4 / 4f;
			}
			return new Quaternion(x, y, z, w);
		}
		
		public static Quaternion SlerpExt(Quaternion a, Quaternion b, float blend) {
			Quaternion result = new Quaternion(0, 0, 0, 1);
			float dot = a.W * b.W + a.X * b.X + a.Y * b.Y + a.Z * b.Z;
			float blendI = 1f - blend;
			if (dot < 0) {
				result.W = blendI * a.W + blend * -b.W;
				result.X = blendI * a.X + blend * -b.X;
				result.Y = blendI * a.Y + blend * -b.Y;
				result.Z = blendI * a.Z + blend * -b.Z;
			} else {
				result.W = blendI * a.W + blend * b.W;
				result.X = blendI * a.X + blend * b.X;
				result.Y = blendI * a.Y + blend * b.Y;
				result.Z = blendI * a.Z + blend * b.Z;
			}
			result.Normalize();
			return result;
		}
        ///<sumary>
        /// Do NOT touch this function. It's not a real ToMatrix, it's an adhoc version.
        /// For a real one look Matrix4.CreateFromQuaterion();
        ///</sumary>
        public static Matrix4 ToMatrix(this Quaternion Quat)
		{
		    float x = Quat.X, y = Quat.Y, z = Quat.Z, w = Quat.W;
		    float xy = x * y;
		    float xz = x * z;
		    float xw = x * w;
		    float yz = y * z;
		    float yw = y * w;
		    float zw = z * w;
		    float xSquared = x * x;
		    float ySquared = y * y;
		    float zSquared = z * z;

		    Matrix4 Matrix = Matrix4.Identity;
		    Matrix.M11 = 1 - 2 * (ySquared + zSquared);
		    Matrix.M12 = 2 * (xy - zw);
		    Matrix.M13 = 2 * (xz + yw);
		    Matrix.M14 = 0;
		    Matrix.M21 = 2 * (xy + zw);
		    Matrix.M22 = 1 - 2 * (xSquared + zSquared);
		    Matrix.M23 = 2 * (yz - xw);
		    Matrix.M24 = 0;
		    Matrix.M31 = 2 * (xz - yw);
		    Matrix.M32 = 2 * (yz + xw);
		    Matrix.M33 = 1 - 2 * (xSquared + ySquared);
		    Matrix.M34 = 0;
		    Matrix.M41 = 0;
		    Matrix.M42 = 0;
		    Matrix.M43 = 0;
		    Matrix.M44 = 1;

		    return Matrix;
        }
		
		public static List<XmlNode> Children(this XmlNode Node, string Name){
			XmlNodeList List = Node.ChildNodes;
			List<XmlNode> NewList = new List<XmlNode>();
			for(int i = 0; i < List.Count; i++){
				if(List[i].Name == Name)
					NewList.Add( List[i] );
			}
			return NewList;
		}
		
		public static int ChildrenCount(this XmlNode Node, string Name){
			XmlNodeList List = Node.ChildNodes;
			int k = 0;
			for(int i = 0; i < List.Count; i++){
				if(List[i].Name == Name)
					k++;
			}
			return k;
		}

		public static XmlNode ChildWithPattern(this XmlNode Node, string Name, string Attribute, string Pattern){
		    for (int j = 0; j < Node.ChildNodes.Count; j++)
		    {
		        if (Node.ChildNodes[j].Name == Name)
		        {
		            for (int i = 0; i < Node.ChildNodes[j].Attributes.Count; i++)
		            {
		                if (Node[Name].Attributes[i].Name == Attribute && Regex.IsMatch(Node.ChildNodes[j].Attributes[i].Value, Pattern))
		                    return Node.ChildNodes[j];
		            }
		        }
		    }
			return null;
		}

	    public static XmlNode ChildWithAttribute(this XmlNode Node, string Name, string Attribute, string Value)
	    {
	        for (int j = 0; j < Node.ChildNodes.Count; j++)
	        {
	            if (Node.ChildNodes[j].Name == Name)
	            {
	                for (int i = 0; i < Node.ChildNodes[j].Attributes.Count; i++)
	                {
	                    if (Node.ChildNodes[j].Attributes[i].Value == Value && Node[Name].Attributes[i].Name == Attribute)
	                        return Node.ChildNodes[j];
	                }
	            }
	        }
	        return null;
	    }

		public static XmlAttribute GetAttribute(this XmlNode Node, string Attr){
			for(int j = 0; j < Node.Attributes.Count; j++){
				if(Node.Attributes[j].Name == Attr)
					return Node.Attributes[j];
			}
			return null;
		}
		
		public static Vector3 ToVector3(this Vector2 Vec2)
	    {
			return new Vector3(Vec2.X, 0, Vec2.Y);
	    }
		
		public static Vector3 Half(this Vector3 Vec3)
	    {
			return new Vector3(Vec3.X*.5f, Vec3.Y*.5f, Vec3.Z*.5f);
	    }
		
		public static Matrix3 ToMatrix3(this Matrix4 Mat4){
			return new Matrix3(Mat4);
		}
		
		public static float Sum(this Vector2 Vec2)
	    {
			return Vec2.X + Vec2.Y;
	    }
		
		public static float Average(this Vector3 Vec3)
	    {
			return (Vec3.X + Vec3.Y + Vec3.Z) / 3;
	    }
		
		public static float Dot(this Vector3 Vec3, Vector3 _Vec3)
	    {
			return Vector3.Dot(Vec3, _Vec3);
	    }
		
		public static Vector3 Cross(this Vector3 Vec3, Vector3 _Vec3)
	    {
			return Vector3.Cross(Vec3, _Vec3);
	    }
		
		public static Vector3 GetSmallest( this Vector3[] Vec3s)
		{
			Vector3 Smallest = Vector3.Zero;
			for(int i = 0; i < Vec3s.Length; i++){
				if(Vec3s[i].LengthSquared < Smallest.LengthSquared)
					Smallest = Vec3s[i];
			}
			return Smallest;
		}
		
		public static Vector3 GetSmallest( this List<Vector3> Vec3s)
		{
			Vector3 Smallest = Vector3.Zero;
			for(int i = 0; i < Vec3s.Count; i++){
				if(Vec3s[i].LengthSquared < Smallest.LengthSquared)
					Smallest = Vec3s[i];
			}
			return Smallest;
		}
		
		public static Vector3 GetBiggest( this Vector3[] Vec3s)
		{
			Vector3 Biggest = Vector3.Zero;
			for(int i = 0; i < Vec3s.Length; i++){
				if(Vec3s[i].LengthSquared > Biggest.LengthSquared)
					Biggest = Vec3s[i];
			}
			return Biggest;
		}
		
		public static Vector3 GetBiggest( this List<Vector3> Vec3s)
		{
			Vector3 Biggest = Vector3.Zero;
			for(int i = 0; i < Vec3s.Count; i++){
				if(Vec3s[i].LengthSquared > Biggest.LengthSquared)
					Biggest = Vec3s[i];
			}
			return Biggest;
		}
		
		public static Vector3 Average(this Vector3[] Vec3)
	    {
			Vector3 Avg = Vector3.Zero;
			for(int i = 0; i < Vec3.Length; i++){
				Avg += Vec3[i];
			}
			Avg /= Vec3.Length;  
			return Avg;
	    }
		
		public static float Distance(this PointF P1, PointF P2){
			PointF P3 = new PointF(P1.X - P2.X, P1.Y - P2.Y);
			return (float) (Math.Sqrt(P3.X * P3.X) + Math.Sqrt(P3.Y * P3.Y));
		}
		
		public static byte[] ToByteArray(this int intValue){
			byte[] intBytes = BitConverter.GetBytes(intValue);
			if (BitConverter.IsLittleEndian)
			    Array.Reverse(intBytes);
			byte[] result = intBytes;
			
			return result;
		}
		
		public static int ToInt32(this byte[] ArrayB){
			if (BitConverter.IsLittleEndian)
			    Array.Reverse(ArrayB);
			return BitConverter.ToInt32(ArrayB,0);
		}
		
		public static List<T> Clone<T>(this List<T> ArrayB){
			return new List<T>(ArrayB);
		}
		
		public static void Write(this BinaryWriter BW, Vector3 Position){
			BW.Write(Position.X);
			BW.Write(Position.Y);
			BW.Write(Position.Z);
		}
		
		public static Vector3 ReadVector3(this BinaryReader BR){
			return new Vector3(BR.ReadSingle(), BR.ReadSingle(), BR.ReadSingle());
		}
		
		public static void Write(this BinaryWriter BW, Vector4 Color){
			BW.Write(Color.X);
			BW.Write(Color.Y);
			BW.Write(Color.Z);
			BW.Write(Color.W);
		}
		
		public static Vector4 ReadVector4(this BinaryReader BR){
			return new Vector4(BR.ReadSingle(), BR.ReadSingle(), BR.ReadSingle(), BR.ReadSingle());
		}

	    public static KeyValuePair<T, R>[] ToArray<T,R>(this Dictionary<T, R> Dictionary)
	    {
	        return Enumerable.ToArray(Dictionary);
	    }

	    public static Dictionary<T, R> FromArray<T, R>(this KeyValuePair<T, R>[] Pairs)
	    {
	        return Pairs.ToDictionary(Pair => Pair.Key, Pair => Pair.Value);
	    }
    }
}
