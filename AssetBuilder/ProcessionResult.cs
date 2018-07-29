using System.IO;
using System.Linq;

namespace AssetBuilder
{
    public class ProcessionResult
    {
        public string Header { get; set; }
        public uint[] Indices { get; set; }
        public Vector3[] Vertices { get; set; }
        public Vector3[] Normals { get; set; }
        public Vector4[] Colors { get; set; }

        public byte[] Serialize()
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write(Header);
                    writer.Write(Indices.Length);
                    for (var i = 0; i < Indices.Length; i++)
                    {
                        writer.Write(Indices[i]);
                    }
                    writer.Write(Vertices.Length);
                    for (var i = 0; i < Vertices.Length; i++)
                    {
                        writer.Write(Vertices[i].X);
                        writer.Write(Vertices[i].Y);
                        writer.Write(Vertices[i].Z);
                    }
                    writer.Write(Normals.Length);
                    for (var i = 0; i < Normals.Length; i++)
                    {
                        writer.Write(Normals[i].X);
                        writer.Write(Normals[i].Y);
                        writer.Write(Normals[i].Z);
                    }
                    writer.Write(Colors.Length);
                    for (var i = 0; i < Colors.Length; i++)
                    {
                        writer.Write(Colors[i].X);
                        writer.Write(Colors[i].Y);
                        writer.Write(Colors[i].Z);
                        writer.Write(Colors[i].W);
                    }
                }
                return ms.ToArray();
            }
        }
    }
    
    public struct Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3(float X, float Y, float Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
    }
	    
    public struct Vector4
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }
		    
        public Vector4(float X, float Y, float Z, float W)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.W = W;
        }
    }
}