using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;

namespace AssetBuilder
{
    public static class PLYProcessor
    {
        public const string Header = "PROCESSEDPLY"; 
        
        private static bool HasColor(string Contents)
        {
            return Contents.Contains("property uchar red") && 
                   Contents.Contains("property uchar green") &&
                   Contents.Contains("property uchar blue");
        }
        
        public static byte[] Process(string Filename)
        {
            string fileContents = File.ReadAllText(Filename);

            int endHeader = fileContents.IndexOf("element vertex", StringComparison.Ordinal);
            fileContents = fileContents.Substring(endHeader, fileContents.Length - endHeader);
            var numbers = Regex.Matches(fileContents, @"-?[\d]+\.[\d]+|[\d]+\.[\d]+|[\d]+").Cast<Match>().Select(M => M.Value).ToArray();

            const int vertexCountIndex = 0;
            const int faceCountIndex = 1;
            const int startDataIndex = 2;

            var hasColor = HasColor(fileContents);
            var hasAlpha = fileContents.IndexOf("property uchar alpha", StringComparison.Ordinal) != -1;
            var vertexCount = int.Parse(numbers[vertexCountIndex]);
            var faceCount = int.Parse(numbers[faceCountIndex]);
            var vertexData = new List<Vector3>(vertexCount);
            var colors = new List<Vector4>();
            var normals = new List<Vector3>();
            var indices = new List<uint>(faceCount * 3);

            var numberOffset = hasColor ? 10 : 6;
            int accumulatedOffset = startDataIndex;
            for(; vertexData.Count < vertexCount; accumulatedOffset += numberOffset)
            {
                vertexData.Add( 
                    new Vector3(
                        float.Parse(numbers[accumulatedOffset + 0], CultureInfo.InvariantCulture),
                        float.Parse(numbers[accumulatedOffset + 1], CultureInfo.InvariantCulture),
                        float.Parse(numbers[accumulatedOffset + 2], CultureInfo.InvariantCulture) 
                        )
                );
                normals.Add( 
                    new Vector3(
                        float.Parse(numbers[accumulatedOffset + 3], CultureInfo.InvariantCulture),
                        float.Parse(numbers[accumulatedOffset + 4], CultureInfo.InvariantCulture),
                        float.Parse(numbers[accumulatedOffset + 5], CultureInfo.InvariantCulture)
                        )
                );
                if (hasColor)
                {
                    colors.Add(
                        new Vector4(
                            float.Parse(numbers[accumulatedOffset + 6]) / 255f,
                            float.Parse(numbers[accumulatedOffset + 7]) / 255f, 
                            float.Parse(numbers[accumulatedOffset + 8]) / 255f,
                            hasAlpha ? float.Parse(numbers[accumulatedOffset + 9]) / 255f : 1.0f
                            )
                    );
                }
            }
            for (; indices.Count / 3 < faceCount; accumulatedOffset += 4)
            {
                indices.Add(uint.Parse(numbers[accumulatedOffset + 1]));
                indices.Add(uint.Parse(numbers[accumulatedOffset + 2]));
                indices.Add(uint.Parse(numbers[accumulatedOffset + 3]));
            }
            return new ProcessionResult
            {
                Header = Header,
                Vertices = vertexData.ToArray(),
                Normals = normals.ToArray(),
                Colors = colors.ToArray(),
                Indices = indices.ToArray()
            }.Serialize();
        }
    }
}