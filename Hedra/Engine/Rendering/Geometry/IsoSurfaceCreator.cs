/*
 * Author: Zaphyk
 * Date: 27/03/2016
 * Time: 10:41 p.m.
 *
 */

using System;
using System.Collections.Generic;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Rendering;
using System.Numerics;

namespace Hedra.Engine.Rendering.Geometry
{
    /// <summary>
    /// Description of MarchingData.
    /// </summary>
    public static class IsoSurfaceCreator
    {
        public const float WaterQuadSize = 1.0f;
        public const float WaterQuadOffset = 1.5f;
        private static Vector3[] FaceUp = new Vector3[4], FaceDown = new Vector3[4], FaceLeft = new Vector3[4],
            FaceRight = new Vector3[4], FaceFront = new Vector3[4], FaceBack = new Vector3[4];
        
        private static Vector3[] FromFace(Vector3 Input, GridCell Cell, int LOD)
        {
            float BlockSize = Chunk.BlockSize;
            if(Vector3.UnitY == Input){
                FaceUp[3] = new Vector3(BlockSize*LOD,0,BlockSize*LOD) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
                FaceUp[2] = new Vector3(BlockSize*LOD,0,0) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
                FaceUp[1] = new Vector3(0,0,BlockSize*LOD) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
                FaceUp[0] = new Vector3(0,0,0) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
                return FaceUp;
            }
            
            if(-Vector3.UnitY == Input){
                FaceDown[3] = new Vector3(BlockSize*LOD,-BlockSize,BlockSize*LOD) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
                FaceDown[2] = new Vector3(BlockSize*LOD,-BlockSize,0) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
                FaceDown[1] = new Vector3(0,-BlockSize,BlockSize*LOD) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
                FaceDown[0] = new Vector3(0,-BlockSize,0) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
                return FaceDown;
            }
            
            if(Vector3.UnitX == Input){
                FaceRight[3] = new Vector3(BlockSize*LOD, 0f, BlockSize*LOD) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
                FaceRight[2] = new Vector3(BlockSize*LOD, -BlockSize, BlockSize*LOD) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
                FaceRight[1] = new Vector3(BlockSize*LOD, 0f, 0f) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
                FaceRight[0] = new Vector3(BlockSize*LOD, -BlockSize, 0f) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
                return FaceRight;
            }
            
            if(Vector3.UnitZ == Input){
                FaceFront[3] = new Vector3(BlockSize*LOD, 0f, BlockSize*LOD) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
                FaceFront[2] = new Vector3(BlockSize*LOD, -BlockSize, BlockSize*LOD) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
                FaceFront[1] = new Vector3(0f, 0f, BlockSize*LOD) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
                FaceFront[0] = new Vector3(0f, -BlockSize, BlockSize*LOD) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
                return FaceFront;
            }
            
            if(-Vector3.UnitX == Input){
                FaceLeft[3] = new Vector3(0f, 0f, BlockSize*LOD) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
                FaceLeft[2] = new Vector3(0f, -BlockSize, BlockSize*LOD) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
                FaceLeft[1] = new Vector3(0f, 0f, 0f) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
                FaceLeft[0] = new Vector3(0f, -BlockSize, 0f) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
                return FaceLeft;
            }
            
            if(-Vector3.UnitZ == Input){
                FaceBack[3] = new Vector3(BlockSize*LOD, 0f, 0f) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
                FaceBack[2] = new Vector3(BlockSize*LOD, -BlockSize, 0f) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
                FaceBack[1] = new Vector3(0f, 0f, 0f) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
                FaceBack[0] = new Vector3(0f, -BlockSize, 0f) + Cell.P[0] * new Vector3(BlockSize,BlockSize,BlockSize);
                return FaceBack;
            }
            
            return null;
        }
        
        public static void CreateWaterQuad(float BlockSize, GridCell Cell, bool Flipped, Vector4 TemplateColor, VertexData Data)
        {
            const int LOD = 1;
            var densities = new []
            {
                (float) Cell.Density[0],
                (float) Cell.Density[1],
                (float) Cell.Density[2],
                (float) Cell.Density[3]
            };

            float Size = WaterQuadSize;
            Vector3 V3 = new Vector3(BlockSize * LOD * Size - (Size-1) * (Cell.P[2].X / Chunk.BlockSize),-Chunk.BlockSize * WaterQuadOffset + (float) (Cell.P[2].Y + densities[2])* Chunk.BlockSize, BlockSize * LOD * Size - (Size-1) * (Cell.P[2].Z / Chunk.BlockSize)) + Cell.P[0].Xz().ToVector3() * new Vector3(BlockSize,BlockSize,BlockSize) * new Vector3(Size,1,Size);
            Vector3 V1 = new Vector3(BlockSize * LOD * Size - (Size-1) * (Cell.P[1].X / Chunk.BlockSize),-Chunk.BlockSize * WaterQuadOffset + (float) (Cell.P[1].Y + densities[1])* Chunk.BlockSize, - (Size-1) * (Cell.P[1].Z / Chunk.BlockSize)) + Cell.P[0].Xz().ToVector3() * new Vector3(BlockSize,BlockSize,BlockSize) * new Vector3(Size,1,Size);
            Vector3 V2 = new Vector3(-(Size-1) * (Cell.P[3].X / Chunk.BlockSize) ,-Chunk.BlockSize * WaterQuadOffset + (float) (Cell.P[3].Y + densities[3])* Chunk.BlockSize, BlockSize * LOD * Size - (Size-1) * (Cell.P[3].Z / Chunk.BlockSize)) + Cell.P[0].Xz().ToVector3() * new Vector3(BlockSize,BlockSize,BlockSize) * new Vector3(Size,1,Size);
            Vector3 V0 = new Vector3(-(Size-1) * (Cell.P[0].X / Chunk.BlockSize), -Chunk.BlockSize * WaterQuadOffset + (float) (Cell.P[0].Y + densities[0])*Chunk.BlockSize, - (Size-1) * (Cell.P[0].Z / Chunk.BlockSize)) + Cell.P[0].Xz().ToVector3() * new Vector3(BlockSize,BlockSize,BlockSize) * new Vector3(Size,1,Size);

            int VertCount = Data.Vertices.Count;
            
            Data.Indices.AddRange( new []
            {
                (uint) Data.Vertices.Count+0,
                (uint) Data.Vertices.Count+1,
                (uint) Data.Vertices.Count+2,
                (uint) Data.Vertices.Count+3,
                (uint) Data.Vertices.Count+4,
                (uint) Data.Vertices.Count+5
            });
            
            if(Flipped){
                
                Data.Vertices.Add(V2);
                Data.Vertices.Add(V3);
                Data.Vertices.Add(V1);
                
                Data.Vertices.Add(V0);
                Data.Vertices.Add(V2);
                Data.Vertices.Add(V1);

            }else{
                
                Data.Vertices.Add(V2);
                Data.Vertices.Add(V3);
                Data.Vertices.Add(V0);
                
                Data.Vertices.Add(V3);
                Data.Vertices.Add(V1);
                Data.Vertices.Add(V0);
            }
            
            Data.Normals.Add(CodeNormal(Data.Vertices[VertCount+0], Data.Vertices[VertCount+1], Data.Vertices[VertCount+2]));
            Data.Normals.Add(CodeNormal(Data.Vertices[VertCount+1], Data.Vertices[VertCount+0], Data.Vertices[VertCount+2]));
            Data.Normals.Add(CodeNormal(Data.Vertices[VertCount+2], Data.Vertices[VertCount+1], Data.Vertices[VertCount+0]));
            
            Data.Normals.Add(CodeNormal(Data.Vertices[VertCount+3], Data.Vertices[VertCount+4],
                                        Data.Vertices[VertCount+5]));
            Data.Normals.Add(CodeNormal(Data.Vertices[VertCount+4], Data.Vertices[VertCount+3],
                                        Data.Vertices[VertCount+5]));
            Data.Normals.Add(CodeNormal(Data.Vertices[VertCount+5], Data.Vertices[VertCount+4],
                                        Data.Vertices[VertCount+3]));
            
            Data.Colors.Add(TemplateColor);
            Data.Colors.Add(TemplateColor);
            Data.Colors.Add(TemplateColor);
            Data.Colors.Add(TemplateColor);
            Data.Colors.Add(TemplateColor);
            Data.Colors.Add(TemplateColor);
        }
        public static Vector3 CodeNormal(Vector3 Original, Vector3 V1, Vector3 V2)
        {
            var coded1 = V1 - Original;
            var coded2 = V2 - Original;
            return new Vector3(Pack(new Vector2(coded1.X, coded2.X), 4096), Pack(new Vector2(coded1.Y, coded2.Y), 4096), Pack(new Vector2(coded1.Z, coded2.Z), 4096));
        }
        
        private static float Pack(Vector2 input, int precision)
        {
            Vector2 output = input;
            output.X = (float) Math.Floor(output.X * (precision - 1));
            output.Y = (float) Math.Floor(output.Y * (precision - 1));
        
            return (output.X * precision) + output.Y;
        }
        
        private static bool ShouldMove(Vector3 Vertex, Vector3 V0, Vector3 V1, Vector3 V2, Vector3 V3, GridCell Cell)
        {
            return true;
        }
    }
}