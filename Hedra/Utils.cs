/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 25/01/2016
 * Time: 10:53 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace Hedra
{
    public static class Utils
    {
        public static Random Rng { get; set; } = new Random();

        public static string FitString(string Input, int CharacterLimit, bool Once = false)
        {
            if (Input == null) return Input;
            var parts = Input.Split(Environment.NewLine.ToCharArray());
            parts = parts.Where(Str => Str != string.Empty).ToArray();
            var atLeast = false;
            var builder = new StringBuilder();
            for (var i = 0; i < parts.Length; i++)
            {
                var originalString = parts[i] + Environment.NewLine;

                while (originalString.Length > CharacterLimit && (Once && !atLeast || !Once))
                {
                    var nearest = FindNearestSeparator(originalString, CharacterLimit - 1);
                    if (Once && originalString.IndexOf(" ") == -1 && parts.Length == 1)
                        if (Math.Abs(nearest) < originalString.Length - nearest)
                            nearest = nearest - 1;
                    //else nearest = originalString.Length - 1;
                    builder.AppendLine(originalString.Substring(0, nearest));
                    originalString = originalString.Substring(nearest, originalString.Length - nearest);
                    atLeast = true;
                }

                builder.Append(originalString);
            }

            return builder.ToString();
        }

        public static Color VariateColor(Color c, int Range)
        {
            var R = (int)(Range * (Rng.NextDouble() * 2 - 1)) + c.R;
            var G = (int)(Range * (Rng.NextDouble() * 2 - 1)) + c.G;
            var B = (int)(Range * (Rng.NextDouble() * 2 - 1)) + c.B;

            if (R >= 255) R = 255;
            if (R <= 0) R = 0;

            if (G >= 255) G = 255;
            if (G <= 0) G = 0;

            if (B >= 255) B = 255;
            if (B <= 0) B = 0;
            return Color.FromArgb(c.A, R, G, B);
        }

        public static Color VariateColor(Color c, int Range, Random Gen)
        {
            var R = (int)(Range * (Gen.NextDouble() * 2 - 1)) + c.R;
            var G = (int)(Range * (Gen.NextDouble() * 2 - 1)) + c.G;
            var B = (int)(Range * (Gen.NextDouble() * 2 - 1)) + c.B;

            if (R >= 255) R = 255;
            if (R <= 0) R = 0;

            if (G >= 255) G = 255;
            if (G <= 0) G = 0;

            if (B >= 255) B = 255;
            if (B <= 0) B = 0;
            return Color.FromArgb(c.A, R, G, B);
        }

        public static Color VariateColor(Color c, int Range, float Val)
        {
            var R = (int)(Range * Val) + c.R;
            var G = (int)(Range * Val) + c.G;
            var B = (int)(Range * Val) + c.B;

            if (R >= 255) R = 255;
            if (R <= 0) R = 0;

            if (G >= 255) G = 255;
            if (G <= 0) G = 0;

            if (B >= 255) B = 255;
            if (B <= 0) B = 0;
            return Color.FromArgb(c.A, R, G, B);
        }

        public static Vector3 VariateColor(Vector3 c, int IRange)
        {
            var Range = IRange / 255f;
            var R = (float)(Range * (Rng.NextDouble() * 2 - 1)) + c.X;
            var G = (float)(Range * (Rng.NextDouble() * 2 - 1)) + c.Y;
            var B = (float)(Range * (Rng.NextDouble() * 2 - 1)) + c.Z;

            if (R >= 1) R = 1;
            if (R <= 0) R = 0;

            if (G >= 1) G = 1;
            if (G <= 0) G = 0;

            if (B >= 1) B = 1;
            if (B <= 0) B = 0;
            return new Vector3(R, G, B);
        }

        public static Vector4 VariateColor(Vector4 c, int IRange)
        {
            var Range = IRange / 255f;
            var R = (float)(Range * (Rng.NextDouble() * 2 - 1)) + c.X;
            var G = (float)(Range * (Rng.NextDouble() * 2 - 1)) + c.Y;
            var B = (float)(Range * (Rng.NextDouble() * 2 - 1)) + c.Z;

            if (R >= 1) R = 1;
            if (R <= 0) R = 0;

            if (G >= 1) G = 1;
            if (G <= 0) G = 0;

            if (B >= 1) B = 1;
            if (B <= 0) B = 0;
            return new Vector4(R, G, B, c.W);
        }

        public static Vector4 VariateColor(Vector4 c, int IRange, Random Gen)
        {
            var Range = IRange / 255f;
            var R = (float)(Range * (Gen.NextDouble() * 2 - 1)) + c.X;
            var G = (float)(Range * (Gen.NextDouble() * 2 - 1)) + c.Y;
            var B = (float)(Range * (Gen.NextDouble() * 2 - 1)) + c.Z;

            if (R >= 1) R = 1;
            if (R <= 0) R = 0;

            if (G >= 1) G = 1;
            if (G <= 0) G = 0;

            if (B >= 1) B = 1;
            if (B <= 0) B = 0;
            return new Vector4(R, G, B, c.W);
        }

        public static Vector4 UniformVariateColor(Vector4 c, int IRange, Random Gen)
        {
            var Range = IRange / 255f;
            var val = Range * ((float)Gen.NextDouble() * 2f - 1f);
            var R = val + c.X;
            var G = val + c.Y;
            var B = val + c.Z;

            if (R >= 1) R = 1;
            if (R <= 0) R = 0;

            if (G >= 1) G = 1;
            if (G <= 0) G = 0;

            if (B >= 1) B = 1;
            if (B <= 0) B = 0;
            return new Vector4(R, G, B, c.W);
        }

        public static string AddSpacesToSentence(this string text, bool preserveAcronyms = false)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            var newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (var i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]) || char.IsNumber(text[i]))
                    if (text[i - 1] != ' ' && !char.IsUpper(text[i - 1]) || char.IsNumber(text[i]) ||
                        preserveAcronyms && char.IsUpper(text[i - 1]) &&
                        i < text.Length - 1 && !char.IsUpper(text[i + 1]) || char.IsNumber(text[i]))
                        newText.Append(' ');

                newText.Append(text[i]);
            }

            return newText.ToString();
        }


        public static bool NextBool(this Random r)
        {
            return r.Next(0, 2) == 1;
        }


        public static IntPtr IntPtrFromByteArray(byte[] Buffer)
        {
            var unmanagedPointer = Marshal.AllocHGlobal(Buffer.Length);
            Marshal.Copy(Buffer, 0, unmanagedPointer, Buffer.Length);
            return unmanagedPointer;
        }

        public static int FindNearestSeparator(string str, int BaseIndex)
        {
            var chars = str.ToCharArray();

            var fwrIndex = BaseIndex;
            for (var i = BaseIndex; i < chars.Length; i++)
                if (chars[i] == ' ' || chars.Length - 1 == i)
                {
                    fwrIndex = i;
                    break;
                }

            var bkwIndex = BaseIndex;
            for (var i = BaseIndex; i > -1; i--)
                if (chars[i] == ' ' || 0 == i)
                {
                    bkwIndex = i;
                    break;
                }

            if (fwrIndex - BaseIndex < BaseIndex - bkwIndex)
                return fwrIndex + 1;
            return bkwIndex + 1;
        }
    }
}