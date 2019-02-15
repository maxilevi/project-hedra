/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 25/01/2016
 * Time: 10:53 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Globalization;
using System.Net;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using OpenTK;

namespace Hedra
{
    public static class Utils
    {

        public static string FitString(string Input, int CharacterLimit, bool Once = false)
        {
            if (Input == null) return Input;
            string[] parts = Input.Split(Environment.NewLine.ToCharArray());
            parts = parts.Where(Str => Str != string.Empty).ToArray();
            var atLeast = false;
            var builder = new StringBuilder();
            for (var i = 0; i < parts.Length; i++)
            {

                string originalString = parts[i] + Environment.NewLine;

                while (originalString.Length > CharacterLimit && (Once && !atLeast || !Once))
                {
                    int nearest = Utils.FindNearestSeparator(originalString, CharacterLimit - 1);
                    if (Once && originalString.IndexOf(" ") == -1 && parts.Length == 1) {
                        if (Math.Abs(nearest) < (originalString.Length - nearest)) nearest = nearest - 1;
                        //else nearest = originalString.Length - 1;
                    }
                    builder.AppendLine(originalString.Substring(0, nearest));
                    originalString = originalString.Substring(nearest, originalString.Length - nearest);
                    atLeast = true;
                }

                builder.Append(originalString);
            }
            return builder.ToString();
        }

        public static Random Rng { get; set; } = new Random();

        public static System.Drawing.Color VariateColor(System.Drawing.Color c, int Range){
            int R = (int)(Range * (Rng.NextDouble() * 2 -1)) + c.R;
            int G = (int)(Range * (Rng.NextDouble() * 2 -1)) + c.G;
            int B = (int)(Range * (Rng.NextDouble() * 2 -1)) + c.B;
            
            if(R >= 255)R = 255;
            if(R <= 0)R = 0;
            
            if(G >= 255)G = 255;
            if(G <= 0)G = 0;
            
            if(B >= 255)B = 255;
            if(B <= 0)B = 0;
            return System.Drawing.Color.FromArgb(c.A, R,G,B);
        }
        
         public static System.Drawing.Color VariateColor(System.Drawing.Color c, int Range, Random Gen){
            int R = (int)(Range * (Gen.NextDouble() * 2 -1)) + c.R;
            int G = (int)(Range * (Gen.NextDouble() * 2 -1)) + c.G;
            int B = (int)(Range * (Gen.NextDouble() * 2 -1)) + c.B;
            
            if(R >= 255)R = 255;
            if(R <= 0)R = 0;
            
            if(G >= 255)G = 255;
            if(G <= 0)G = 0;
            
            if(B >= 255)B = 255;
            if(B <= 0)B = 0;
            return System.Drawing.Color.FromArgb(c.A, R,G,B);
        }
        
         public static System.Drawing.Color VariateColor(System.Drawing.Color c, int Range, float Val){
            int R = (int)(Range * Val) + c.R;
            int G = (int)(Range * Val) + c.G;
            int B = (int)(Range * Val) + c.B;
            
            if(R >= 255)R = 255;
            if(R <= 0)R = 0;
            
            if(G >= 255)G = 255;
            if(G <= 0)G = 0;
            
            if(B >= 255)B = 255;
            if(B <= 0)B = 0;
            return System.Drawing.Color.FromArgb(c.A, R,G,B);
        }
        
        public static Vector3 VariateColor(Vector3 c, int IRange){
            float Range = IRange / 255f;
            float R = (float) (Range * (Rng.NextDouble() * 2 -1)) + c.X;
            float G = (float) (Range * (Rng.NextDouble() * 2 -1)) + c.Y;
            float B = (float) (Range * (Rng.NextDouble() * 2 -1)) + c.Z;
            
            if(R >= 1)R = 1;
            if(R <= 0)R = 0;
            
            if(G >= 1)G = 1;
            if(G <= 0)G = 0;
            
            if(B >= 1)B = 1;
            if(B <= 0)B = 0;
            return new Vector3(R, G, B);
        }
        
        public static Vector4 VariateColor(Vector4 c, int IRange){
            float Range = IRange / 255f;
            float R = (float) (Range * (Rng.NextDouble() * 2 -1)) + c.X;
            float G = (float) (Range * (Rng.NextDouble() * 2 -1)) + c.Y;
            float B = (float) (Range * (Rng.NextDouble() * 2 -1)) + c.Z;
            
            if(R >= 1)R = 1;
            if(R <= 0)R = 0;
            
            if(G >= 1)G = 1;
            if(G <= 0)G = 0;
            
            if(B >= 1)B = 1;
            if(B <= 0)B = 0;
            return new Vector4(R, G, B, c.W);
        }
        
         public static Vector4 VariateColor(Vector4 c, int IRange, Random Gen){
            float Range = IRange / 255f;
            float R = (float) (Range * (Gen.NextDouble() * 2 -1)) + c.X;
            float G = (float) (Range * (Gen.NextDouble() * 2 -1)) + c.Y;
            float B = (float) (Range * (Gen.NextDouble() * 2 -1)) + c.Z;
            
            if(R >= 1)R = 1;
            if(R <= 0)R = 0;
            
            if(G >= 1)G = 1;
            if(G <= 0)G = 0;
            
            if(B >= 1)B = 1;
            if(B <= 0)B = 0;
            return new Vector4(R, G, B, c.W);
        }
        
         public static Vector4 UniformVariateColor(Vector4 c, int IRange, Random Gen){
            float Range = IRange / 255f;
            float val = Range * ( (float) Gen.NextDouble() * 2f -1f);
            float R = (float) val + c.X;
            float G = (float) val + c.Y;
            float B = (float) val + c.Z;
            
            if(R >= 1)R = 1;
            if(R <= 0)R = 0;
            
            if(G >= 1)G = 1;
            if(G <= 0)G = 0;
            
            if(B >= 1)B = 1;
            if(B <= 0)B = 0;
            return new Vector4(R, G, B, c.W);
        }
        
        public static string AddSpacesToSentence(this string text, bool preserveAcronyms = false)
        {
            if (string.IsNullOrWhiteSpace(text))
               return string.Empty;
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]) || char.IsNumber(text[i]))
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])  || char.IsNumber(text[i])) ||
                        (preserveAcronyms && char.IsUpper(text[i - 1]) && 
                         i < text.Length - 1 && !char.IsUpper(text[i + 1])) || char.IsNumber(text[i]))
                        newText.Append(' ');
                
                newText.Append(text[i]);
            }
            return newText.ToString();
        }

        
        public static  bool NextBool(this Random r)
        {
            return r.Next(0,2) == 1;
        }

        
        public static IntPtr IntPtrFromByteArray(byte[] Buffer){
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(Buffer.Length);
            Marshal.Copy(Buffer, 0, unmanagedPointer, Buffer.Length);
            return unmanagedPointer;
        }
        
        public static int FindNearestSeparator(string str, int BaseIndex)
        {
            char[] chars = str.ToCharArray();
            
            int fwrIndex = BaseIndex;
            for(int i = BaseIndex; i < chars.Length; i++){
                if( chars[i] == ' ' || chars.Length-1 == i){ 
                    fwrIndex = i;
                    break;
                }
            }
            
            int bkwIndex = BaseIndex;
            for(int i = BaseIndex; i > -1; i--)
            {
                if( chars[i] == ' ' || 0 == i)
                { 
                    bkwIndex = i;
                    break;
                }
            }
            
            if(fwrIndex - BaseIndex < BaseIndex - bkwIndex)
                return fwrIndex+1;
            else
                return bkwIndex+1;
        } 
    }
}
