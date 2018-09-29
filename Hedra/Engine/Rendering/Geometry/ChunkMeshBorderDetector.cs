using System;
using System.Collections.Generic;
using OpenTK;

namespace Hedra.Engine.Rendering.Geometry
{
    public class ChunkMeshBorderDetector
    {
        private const double Epsilon = 0.0005;

        public MeshBorderOutput Process(VertexData TerrainMesh, Vector3 Size)
        {
            return Process(TerrainMesh.Vertices.ToArray(), Size);
        }

        public MeshBorderOutput Process(Vector3[] TerrainMesh, Vector3 Size)
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
                
                if(Math.Abs(vertex.X) < Epsilon && Math.Abs(vertex.Z) < Epsilon) backLeftCorner.Add(vertex);
                else if(Math.Abs(vertex.X - Size.X) < .005 && Math.Abs(vertex.Z) < Epsilon) backRightCorner.Add(vertex);
                else if(Math.Abs(vertex.X) < Epsilon && Math.Abs(vertex.Z - Size.Z) < Epsilon) frontLeftCorner.Add(vertex);
                else if(Math.Abs(vertex.X - Size.X) < Epsilon && Math.Abs(vertex.Z - Size.Z) < Epsilon) frontRightCorner.Add(vertex);

                else if(Math.Abs(vertex.X) < Epsilon) left.Add(vertex);
                else if(Math.Abs(vertex.X - Size.X) < Epsilon) right.Add(vertex);
                else if(Math.Abs(vertex.Z) < Epsilon) back.Add(vertex);
                else if(Math.Abs(vertex.Z - Size.Z) < Epsilon) front.Add(vertex);
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