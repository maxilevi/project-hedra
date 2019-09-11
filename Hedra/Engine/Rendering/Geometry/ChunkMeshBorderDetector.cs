using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Rendering;
using IronPython.Modules;
using OpenTK;

namespace Hedra.Engine.Rendering.Geometry
{
    public class ChunkMeshBorderDetector
    {
        private const double Epsilon = 0.005f;

        public MeshBorderOutput Process(VertexData TerrainMesh,Vector3 Start, Vector3 End)
        {
            return Process(TerrainMesh.Vertices.ToArray(), Start, End);
        }


        public MeshBorderOutput Process(VertexData TerrainMesh, Vector3 Size)
        {
            return Process(TerrainMesh.Vertices.ToArray(), Vector3.Zero, Size);
        }

        public MeshBorderOutput Process(Vector3[] TerrainMesh, Vector3 Start, Vector3 End)
        {
            var front = new List<Vector3>(); 
            var back = new List<Vector3>();
            var right = new List<Vector3>();
            var left = new List<Vector3>();
            
            var frontRightCorner = new List<Vector3>(); 
            var frontLeftCorner = new List<Vector3>();
            var backRightCorner = new List<Vector3>();
            var backLeftCorner = new List<Vector3>();
            for (var i = 0; i < TerrainMesh.Length; i++)
            {
                var vertex = TerrainMesh[i];
                
                if(Math.Abs(vertex.X - Start.X) < Epsilon && Math.Abs(vertex.Z - Start.X) < Epsilon) backLeftCorner.Add(vertex);
                else if(Math.Abs(vertex.X - End.X) < Epsilon && Math.Abs(vertex.Z - Start.Z) < Epsilon) backRightCorner.Add(vertex);
                else if(Math.Abs(vertex.X - Start.X) < Epsilon && Math.Abs(vertex.Z - End.Z) < Epsilon) frontLeftCorner.Add(vertex);
                else if(Math.Abs(vertex.X - End.X) < Epsilon && Math.Abs(vertex.Z - End.Z) < Epsilon) frontRightCorner.Add(vertex);

                else if(Math.Abs(vertex.X - Start.X) < Epsilon && vertex.Z > Start.Z && vertex.Z < End.Z) left.Add(vertex);
                else if(Math.Abs(vertex.X - End.X) < Epsilon && vertex.Z > Start.Z && vertex.Z < End.Z) right.Add(vertex);
                else if(Math.Abs(vertex.Z - Start.Z) < Epsilon && vertex.X > Start.X && vertex.X < End.X) back.Add(vertex);
                else if(Math.Abs(vertex.Z - End.Z) < Epsilon && vertex.X > Start.X && vertex.X < End.X) front.Add(vertex);
            }
            return new MeshBorderOutput
            {
                LeftBorder = left.ToArray(),
                BackBorder = back.ToArray(),
                RightBorder = right.ToArray(),
                FrontBorder = front.ToArray(),
                
                BackLeftCorner = backLeftCorner.ToArray(),
                BackRightCorner = backRightCorner.ToArray(),
                FrontRightCorner = frontRightCorner.ToArray(),
                FrontLeftCorner = frontLeftCorner.ToArray()
            };
        }
        
        public uint[] ProcessEntireBorder(VertexData Mesh, Vector3 Start, Vector3 End)
        {
            const float epsilon = 0.5f;
            var all = new List<uint>();
            for (var i = 0; i < Mesh.Indices.Count; i++)
            {
                var index = (int) Mesh.Indices[i];
                var vertex = Mesh.Vertices[index];
                if(Math.Abs(vertex.Z - Start.Z) < epsilon || Math.Abs(vertex.Z - End.Z) < epsilon || Math.Abs(vertex.X - Start.X) < epsilon || Math.Abs(vertex.X - End.X) < epsilon)
                    all.Add((uint)index);
            }

            return all.ToArray();
        }
    }

    public class MeshBorderOutput
    {
        public Vector3[] FrontBorder { get; set; }
        public Vector3[] LeftBorder { get; set; }
        public Vector3[] RightBorder { get; set; }
        public Vector3[] BackBorder { get; set; }
        
        public Vector3[] FrontRightCorner { get; set; }
        public Vector3[] FrontLeftCorner { get; set; }
        public Vector3[] BackRightCorner { get; set; }
        public Vector3[] BackLeftCorner { get; set; }

        public Vector3[] All => FrontBorder.Concat(LeftBorder).Concat(RightBorder).Concat(BackBorder)
            .Concat(FrontRightCorner).Concat(FrontLeftCorner).Concat(BackRightCorner).Concat(BackLeftCorner).ToArray();

        public override bool Equals(object Obj)
        {
            if (ReferenceEquals(null, Obj)) return false;
            if (ReferenceEquals(this, Obj)) return true;
            if (Obj.GetType() != GetType()) return false;
            var borderOutput = (MeshBorderOutput) Obj;
            return EqualElements(borderOutput.FrontBorder, FrontBorder) &&
                   EqualElements(borderOutput.LeftBorder, LeftBorder) &&
                   EqualElements(borderOutput.RightBorder, RightBorder) &&
                   EqualElements(borderOutput.BackBorder, BackBorder) &&
                   EqualElements(borderOutput.FrontRightCorner, FrontRightCorner) &&
                   EqualElements(borderOutput.FrontLeftCorner, FrontLeftCorner) &&
                   EqualElements(borderOutput.BackRightCorner, BackRightCorner) &&
                   EqualElements(borderOutput.BackLeftCorner, BackLeftCorner);
        }

        private static bool EqualElements(Vector3[] A, Vector3[] B)
        {
            if (A.Length == B.Length)
            {
                for (var i = 0; i < A.Length; i++)
                {
                    if (A[i] != B[i]) return false;
                }
                return true;
            }
            return false;
        }
    }
}