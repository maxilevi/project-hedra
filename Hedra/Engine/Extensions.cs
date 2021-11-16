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
using SixLabors.ImageSharp;
using SixLabors.Fonts;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Hedra.Core;
using Hedra.Engine.Core;
using Newtonsoft.Json.Converters;
using System.Numerics;
using Hedra.Numerics;
using Hedra.Framework;

namespace Hedra.Engine
{
    /// <summary>
    /// Description of Extensions.
    /// </summary>
    public static class Extensions
    {
        public static SequentialList<T> ToSequential<T>(List<T> List)
        {
            return new SequentialList<T>(List);
        }

        public static float Clamp01(this float Value)
        {
            return Mathf.Clamp(Value, 0, 1);
        }

        public static float Clamp0(this float Value)
        {
            return Math.Max(Value, 0);
        }

        public static int Clamp0(this int Value)
        {
            return Math.Max(Value, 0);
        }

        public static void Set<T>(this List<T> List, T Value, int Times)
        {
            List.Clear();
            List.Capacity = Times;
            for (var i = 0; i < Times; ++i)
                List.Add(Value);
        }

        public static void Shuffle<T>(this IList<T> List, Random Rng)
        {
            var n = List.Count;
            while (n > 1)
            {
                n--;
                var k = Rng.Next(n + 1);
                var value = List[k];
                List[k] = List[n];
                List[n] = value;
            }
        }

        public static T Random<T>(this IList<T> List, Random Rng)
        {
            return List[Rng.Next(0, List.Count)];
        }

        public static Vector3 SupportPoint(this Vector3[] Vertices, Vector3 Direction)
        {
            var highest = float.MinValue;
            var support = Vector3.Zero;
            for (var i = 0; i < Vertices.Length; i++)
            {
                var dot = Vector3.Dot(Direction, Vertices[i]);
                if (dot > highest)
                {
                    highest = dot;
                    support = Vertices[i];
                }
            }

            return support;
        }

        public static bool IsInvalid(this Vector3 Vector)
        {
            return Vector.X.IsInvalid() || Vector.Y.IsInvalid() || Vector.Z.IsInvalid();
        }

        public static bool IsInvalid(this float Value)
        {
            return float.IsNaN(Value) || float.IsInfinity(Value) || Value > int.MaxValue || Value < int.MinValue;
        }

        public static Vector3 ToEuler(this Quaternion Quaternion)
        {
            return QuaternionMath.ToEuler(Quaternion);
        }

        public static List<T> DeepClone<T>(this List<T> List)
        {
            if (List.Count == 0) return new List<T>();

            if (List[0] is ICloneable)
            {
                var newList = new List<T>();
                for (var i = 0; i < List.Count; i++) newList.Add((T)(List[i] as ICloneable)?.Clone());
                return newList;
            }

            throw new ArgumentException($"Cannot create a deep clone of Unclonable object");
        }

        public static Quaternion FromMatrixExt(Matrix4x4 matrix)
        {
            float w, x, y, z;
            var diagonal = matrix.M11 + matrix.M22 + matrix.M33;
            if (diagonal > 0)
            {
                var w4 = (float)(Math.Sqrt(diagonal + 1f) * 2f);
                w = w4 / 4f;
                x = (matrix.M32 - matrix.M23) / w4;
                y = (matrix.M13 - matrix.M31) / w4;
                z = (matrix.M21 - matrix.M12) / w4;
            }
            else if (matrix.M11 > matrix.M22 && matrix.M11 > matrix.M33)
            {
                var x4 = (float)(Math.Sqrt(1f + matrix.M11 - matrix.M22 - matrix.M33) * 2f);
                w = (matrix.M32 - matrix.M23) / x4;
                x = x4 / 4f;
                y = (matrix.M12 + matrix.M21) / x4;
                z = (matrix.M13 + matrix.M31) / x4;
            }
            else if (matrix.M22 > matrix.M33)
            {
                var y4 = (float)(Math.Sqrt(1f + matrix.M22 - matrix.M11 - matrix.M33) * 2f);
                w = (matrix.M13 - matrix.M31) / y4;
                x = (matrix.M12 + matrix.M21) / y4;
                y = y4 / 4f;
                z = (matrix.M23 + matrix.M32) / y4;
            }
            else
            {
                var z4 = (float)(Math.Sqrt(1f + matrix.M33 - matrix.M11 - matrix.M22) * 2f);
                w = (matrix.M21 - matrix.M12) / z4;
                x = (matrix.M13 + matrix.M31) / z4;
                y = (matrix.M23 + matrix.M32) / z4;
                z = z4 / 4f;
            }

            return new Quaternion(x, y, z, w);
        }

        public static Quaternion SlerpExt(Quaternion a, Quaternion b, float blend)
        {
            var result = new Quaternion(0, 0, 0, 1);
            var dot = a.W * b.W + a.X * b.X + a.Y * b.Y + a.Z * b.Z;
            var blendI = 1f - blend;
            if (dot < 0)
            {
                result.W = blendI * a.W + blend * -b.W;
                result.X = blendI * a.X + blend * -b.X;
                result.Y = blendI * a.Y + blend * -b.Y;
                result.Z = blendI * a.Z + blend * -b.Z;
            }
            else
            {
                result.W = blendI * a.W + blend * b.W;
                result.X = blendI * a.X + blend * b.X;
                result.Y = blendI * a.Y + blend * b.Y;
                result.Z = blendI * a.Z + blend * b.Z;
            }

            return result.NormalizedFast();
        }

        ///<sumary>
        /// Do NOT touch this function. It's not a real ToMatrix, it's an adhoc version.
        /// For a real one look Matrix4x4.CreateFromQuaterion();
        ///</sumary>
        public static Matrix4x4 ToMatrix(this Quaternion Quat)
        {
            float x = Quat.X, y = Quat.Y, z = Quat.Z, w = Quat.W;
            var xy = x * y;
            var xz = x * z;
            var xw = x * w;
            var yz = y * z;
            var yw = y * w;
            var zw = z * w;
            var xSquared = x * x;
            var ySquared = y * y;
            var zSquared = z * z;

            var Matrix = Matrix4x4.Identity;
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

        public static List<XmlNode> Children(this XmlNode Node, string Name)
        {
            var List = Node.ChildNodes;
            var NewList = new List<XmlNode>();
            for (var i = 0; i < List.Count; i++)
                if (List[i].Name == Name)
                    NewList.Add(List[i]);
            return NewList;
        }

        public static int ChildrenCount(this XmlNode Node, string Name)
        {
            var List = Node.ChildNodes;
            var k = 0;
            for (var i = 0; i < List.Count; i++)
                if (List[i].Name == Name)
                    k++;
            return k;
        }

        public static XmlNode ChildWithPattern(this XmlNode Node, string Name, string Attribute, string Pattern)
        {
            for (var j = 0; j < Node.ChildNodes.Count; j++)
                if (Node.ChildNodes[j].Name == Name)
                    for (var i = 0; i < Node.ChildNodes[j].Attributes.Count; i++)
                        if (Node[Name].Attributes[i].Name == Attribute &&
                            Regex.IsMatch(Node.ChildNodes[j].Attributes[i].Value, Pattern))
                            return Node.ChildNodes[j];
            return null;
        }

        public static XmlNode ChildWithAttribute(this XmlNode Node, string Name, string Attribute, string Value)
        {
            for (var j = 0; j < Node.ChildNodes.Count; j++)
                if (Node.ChildNodes[j].Name == Name)
                    for (var i = 0; i < Node.ChildNodes[j].Attributes.Count; i++)
                        if (Node.ChildNodes[j].Attributes[i].Value == Value &&
                            Node[Name].Attributes[i].Name == Attribute)
                            return Node.ChildNodes[j];
            return null;
        }

        public static XmlAttribute GetAttribute(this XmlNode Node, string Attr)
        {
            for (var j = 0; j < Node.Attributes.Count; j++)
                if (Node.Attributes[j].Name == Attr)
                    return Node.Attributes[j];
            return null;
        }

        public static Vector3 ToVector3(this Vector2 Vec2)
        {
            return new Vector3(Vec2.X, 0, Vec2.Y);
        }


        public static float Average(this Vector3 Vec3)
        {
            return (Vec3.X + Vec3.Y + Vec3.Z) / 3;
        }

        public static float Dot(this Vector3 Vec3, Vector3 _Vec3)
        {
            return Vec3.X * _Vec3.X + Vec3.Y * _Vec3.Y + Vec3.Z * _Vec3.Z;
        }

        public static Vector3 Cross(this Vector3 Left, Vector3 Right)
        {
            return new Vector3
            {
                X = Left.Y * Right.Z - Left.Z * Right.Y,
                Y = Left.Z * Right.X - Left.X * Right.Z,
                Z = Left.X * Right.Y - Left.Y * Right.X
            };
        }

        public static List<T> Clone<T>(this List<T> ArrayB)
        {
            return new List<T>(ArrayB);
        }

        public static NativeArray<T> ToNativeArray<T>(this List<T> List, IAllocator Allocator) where T : unmanaged
        {
            var array = new NativeArray<T>(Allocator, List.Count);
            for (var i = 0; i < List.Count; ++i) array[i] = List[i];
            return array;
        }

        public static NativeList<T> NativeClone<T>(this List<T> List, IAllocator Allocator) where T : unmanaged
        {
            return new NativeList<T>(Allocator);
        }

        public static void Write(this BinaryWriter BW, Vector3 Position)
        {
            BW.Write(Position.X);
            BW.Write(Position.Y);
            BW.Write(Position.Z);
        }

        public static Vector3 ReadVector3(this BinaryReader BR)
        {
            return new Vector3(BR.ReadSingle(), BR.ReadSingle(), BR.ReadSingle());
        }

        public static void Write(this BinaryWriter BW, Vector4 Color)
        {
            BW.Write(Color.X);
            BW.Write(Color.Y);
            BW.Write(Color.Z);
            BW.Write(Color.W);
        }

        public static Vector4 ReadVector4(this BinaryReader BR)
        {
            return new Vector4(BR.ReadSingle(), BR.ReadSingle(), BR.ReadSingle(), BR.ReadSingle());
        }

        public static KeyValuePair<T, R>[] ToArray<T, R>(this Dictionary<T, R> Dictionary)
        {
            return Enumerable.ToArray(Dictionary);
        }

        public static Dictionary<T, R> FromArray<T, R>(this KeyValuePair<T, R>[] Pairs)
        {
            return Pairs.ToDictionary(Pair => Pair.Key, Pair => Pair.Value);
        }
    }
}