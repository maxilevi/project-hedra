/*
 * Author: Zaphyk
 * Date: 27/03/2016
 * Time: 10:03 p.m.
 *
 */

using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BulletSharp;
using Facepunch.Steamworks;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Numerics;
using Chunk = Hedra.Engine.Generation.ChunkSystem.Chunk;

namespace Hedra.Engine.Rendering
{
    /// <summary>
    ///     Description of MarchingCubes.
    /// </summary>
    public static class MarchingCubes
    {
        const float IsoLevel = 0f;
        const int TriangulationTableLength = 16;

        private static int ShuffleTriangulationIndexBits(int i)
        {
	        // default cornermask :	76 54 32 10
	        // new order :			67 23 54 10
	        //
	        // 0 => 0
	        // 1 => 1
	        // 2 => 5
	        // 3 => 4
	        // 4 => 2
	        // 5 => 3
	        // 6 => 7
	        // 7 => 6

	        int ret = 0;
	        if ((i & 1 << 0) != 0) ret |= 1 << 0;
	        if ((i & 1 << 1) != 0) ret |= 1 << 1;
	        if ((i & 1 << 2) != 0) ret |= 1 << 5;
	        if ((i & 1 << 3) != 0) ret |= 1 << 4;
	        if ((i & 1 << 4) != 0) ret |= 1 << 2;
	        if ((i & 1 << 5) != 0) ret |= 1 << 3;
	        if ((i & 1 << 6) != 0) ret |= 1 << 7;
	        if ((i & 1 << 7) != 0) ret |= 1 << 6;
	        return ret;
        }

        private static byte[] ShuffledTriangulationTable;

        private static void GetTriangulationTable()
        {
	        if (ShuffledTriangulationTable != null) return;
	        ShuffledTriangulationTable = new byte[TriangulationTable.Length];
	        var table = ShuffledTriangulationTable;
	        for (int i = 0; i < 256; i++)
	        {
		        var j = ShuffleTriangulationIndexBits(i);
		        for (int k = 0; k < TriangulationTableLength; k++)
		        {
			        table[j * TriangulationTableLength + k] = TriangulationTable[i * TriangulationTableLength + k];
		        }
	        }
        }

        private unsafe static void buildVolumePtr(sbyte* grid, SampledBlock* arr)
        {
	        for (var i = 0; i < ChunkTerrainMeshBuilderHelper.CalculateGridSize(1); i++)
	        {
		        grid[i] = (sbyte) (Mathf.Clamp(arr[i].Density, -1, 1) * -127);
	        }
        }

        public static unsafe void LoopSIMD(ref NativeVertexData blockData, SampledBlock* noiseData)
        {
	        var buffer = NewTriangleBuffer();
	        var volumePtr = stackalloc sbyte[ChunkTerrainMeshBuilderHelper.CalculateGridSize(1)];
	        GetTriangulationTable();
	        buildVolumePtr(volumePtr, noiseData);
	        var triangulationTable = ShuffledTriangulationTable;			
			
			if (Chunk.BoundsZ != 32)
				throw new System.Exception("ChunkSize Z must be equal to 32 to use this function");

            var xStart = 0;
            var xStop = Chunk.BoundsZ-1;

			sbyte* samplesBase = stackalloc sbyte[Chunk.BoundsZ * 4];
			sbyte* samples01 = samplesBase + Chunk.BoundsZ * 0;
			sbyte* samples23 = samplesBase + Chunk.BoundsZ * 2;

			int signBits0, signBits1, signBits2, signBits3;

			var samples = stackalloc Vector4[8];

			for (int x = xStart; x < xStop; x++)
			{
				(signBits2, signBits3) = SimdExtractBitsAndSamples(samples23, volumePtr, x);

				for (int y = 0; y < Chunk.BoundsY - 1; y++)
				{
					// reuse previous step
					var temp = samples01;
					samples01 = samples23;
					samples23 = temp;
					signBits0 = signBits2;
					signBits1 = signBits3;


					(signBits2, signBits3) = SimdExtractBitsAndSamples(samples23, volumePtr, x, y);


					var signBits = Vector128.Create(signBits0, signBits1, signBits2, signBits3);


					if (SameSigns(signBits))
						continue;


					int cornerMask = Sse.MoveMask(signBits.AsSingle()) << 4;

					for (int z = 0; z < Chunk.BoundsZ - 1; z++)
					{
						cornerMask >>= 4;
						signBits = Sse2.ShiftLeftLogical(signBits, 1);
						cornerMask |= Sse.MoveMask(signBits.AsSingle()) << 4;

						if (cornerMask == 0 || cornerMask == 255)
							continue;


						var zz = z + z;
						var unit = 1;
						var scale = new Vector4(Chunk.BlockSize, 1, Chunk.BlockSize, 1);
						samples[0] = new Vector4(x + 0, y + 0, z + 0, samples01[zz + 0]) * scale;
						samples[1] = new Vector4(x + 1, y + 0, z + 0, samples01[zz + 1]) * scale;
						samples[2] = new Vector4(x + 1, y + 0, z + 1, samples01[zz + 3]) * scale;
						samples[3] = new Vector4(x + 0, y + 0, z + 1, samples01[zz + 2]) * scale;
						samples[4] = new Vector4(x + 0, y + 1, z + 0, samples23[zz + 0]) * scale;
						samples[5] = new Vector4(x + 1, y + 1, z + 0, samples23[zz + 1]) * scale;
						samples[6] = new Vector4(x + 1, y + 1, z + 1, samples23[zz + 3]) * scale;
						samples[7] = new Vector4(x + 0, y + 1, z + 1, samples23[zz + 2]) * scale;

						var triangulationTableIndex = cornerMask * TriangulationTableLength;

						//bounds.item.Encapsulate(new Vector3(x, y, z));
						//bounds.item.Encapsulate(new Vector3(x + 1, y + 1, z + 1));
						var triangleCount = 0;
						for (; triangulationTable[triangulationTableIndex] != 99; triangulationTableIndex += 3)
						{
							int a0 = CornerIndexA[triangulationTable[triangulationTableIndex]];
							int b0 = CornerIndexB[triangulationTable[triangulationTableIndex]];
							int a1 = CornerIndexA[triangulationTable[triangulationTableIndex + 1]];
							int b1 = CornerIndexB[triangulationTable[triangulationTableIndex + 1]];
							int a2 = CornerIndexA[triangulationTable[triangulationTableIndex + 2]];
							int b2 = CornerIndexB[triangulationTable[triangulationTableIndex + 2]];
							
							buffer[triangleCount].Vertices[0] = InterpolateVerts(samples[a0], samples[b0]);
							buffer[triangleCount].Vertices[1] = InterpolateVerts(samples[a1], samples[b1]);
							buffer[triangleCount].Vertices[2] = InterpolateVerts(samples[a2], samples[b2]);
							triangleCount++;
						}

						var color = new Vector4(0.5f, 0.5f, 0.5f, 1f);
						var isRiverConstant = false;
						var isWater = false;
						Build(ref blockData, ref color, ref buffer, ref triangleCount, ref isWater, ref isRiverConstant);
					}
				}
			}
		}
        
        private static unsafe (int, int) SimdExtractBitsAndSamples(sbyte* samples23, sbyte* volumePtr, int x, int y = -1 /* first step */)
        {
            var shuffleReverseByteOrder = Vector128.Create(15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0);
//x * noiseValuesMapHeight * noiseValuesMapWidth + y * noiseValuesMapWidth + z
            var ptr2 = (sbyte*)volumePtr + (x + 0) * Chunk.BoundsY * Chunk.BoundsZ + (y + 1) * Chunk.BoundsZ;
            var ptr3 = (sbyte*)volumePtr + (x + 1) * Chunk.BoundsY * Chunk.BoundsZ + (y + 1) * Chunk.BoundsZ;
            var lo2 = Sse2.LoadAlignedVector128(ptr2 + 0);
            var hi2 = Sse2.LoadAlignedVector128(ptr2 + 16);
            var lo3 = Sse2.LoadAlignedVector128(ptr3 + 0);
            var hi3 = Sse2.LoadAlignedVector128(ptr3 + 16);
            Sse2.StoreAligned(samples23 + 00, Sse2.UnpackLow(lo2, lo3));
            Sse2.StoreAligned(samples23 + 16, Sse2.UnpackHigh(lo2, lo3));
            Sse2.StoreAligned(samples23 + 32, Sse2.UnpackLow(hi2, hi3));
            Sse2.StoreAligned(samples23 + 48, Sse2.UnpackHigh(hi2, hi3));
            lo2 = Ssse3.Shuffle(lo2, shuffleReverseByteOrder);
            lo3 = Ssse3.Shuffle(lo3, shuffleReverseByteOrder);
            hi2 = Ssse3.Shuffle(hi2, shuffleReverseByteOrder);
            hi3 = Ssse3.Shuffle(hi3, shuffleReverseByteOrder);
            var signBits2 = (Sse2.MoveMask(lo2) << 16 | (Sse2.MoveMask(hi2)));
            var signBits3 = (Sse2.MoveMask(lo3) << 16 | (Sse2.MoveMask(hi3)));
            return (signBits2, signBits3);
        }

        private static Vector3 InterpolateVerts(Vector4 v1, Vector4 v2)
        {
	        float t = (IsoLevel - v1.W) / (v2.W - v1.W);
	        return (v1 + t * (v2 - v1)).Xyz();
        }
        
        private static bool SameSigns(Vector128<int> signBits)
        {
	        var maskAllOnes = Vector128<int>.AllBitsSet;
	        return Sse42.TestNotZAndNotC(signBits, maskAllOnes) == false;
        }
        
        
        #region AXULIARIES

        public static bool Usable(GridCell Cell)
        {
            byte CubeIndex = 0;
            if (Cell.Density[0] > IsoLevel) CubeIndex |= 1;
            if (Cell.Density[1] > IsoLevel) CubeIndex |= 2;
            if (Cell.Density[2] > IsoLevel) CubeIndex |= 4;
            if (Cell.Density[3] > IsoLevel) CubeIndex |= 8;
            if (Cell.Density[4] > IsoLevel) CubeIndex |= 16;
            if (Cell.Density[5] > IsoLevel) CubeIndex |= 32;
            if (Cell.Density[6] > IsoLevel) CubeIndex |= 64;
            if (Cell.Density[7] > IsoLevel) CubeIndex |= 128;

            return EdgeTable[CubeIndex] != 0;
        }

        #endregion

        public static Triangle[] NewTriangleBuffer()
        {
            return new[]
            {
                new Triangle { Vertices = new Vector3[3] },
                new Triangle { Vertices = new Vector3[3] },
                new Triangle { Vertices = new Vector3[3] },
                new Triangle { Vertices = new Vector3[3] },
                new Triangle { Vertices = new Vector3[3] },
                new Triangle { Vertices = new Vector3[3] },
                new Triangle { Vertices = new Vector3[3] },
                new Triangle { Vertices = new Vector3[3] },
                new Triangle { Vertices = new Vector3[3] }
            };
        }

        public static Vector3[] NewVertexBuffer()
        {
            return new Vector3[12];
        }


        public static void Polygonise(ref GridCell Cell, ref Vector3[] VertexBuffer,
            ref Triangle[] TriangleBuffer, out int TriangleCount)
        {
            TriangleCount = 0;
            Debug.Assert(VertexBuffer.Length == 12);
            byte cubeIndex = 0;
            if (Cell.Density[0] > IsoLevel) cubeIndex |= 1;
            if (Cell.Density[1] > IsoLevel) cubeIndex |= 2;
            if (Cell.Density[2] > IsoLevel) cubeIndex |= 4;
            if (Cell.Density[3] > IsoLevel) cubeIndex |= 8;
            if (Cell.Density[4] > IsoLevel) cubeIndex |= 16;
            if (Cell.Density[5] > IsoLevel) cubeIndex |= 32;
            if (Cell.Density[6] > IsoLevel) cubeIndex |= 64;
            if (Cell.Density[7] > IsoLevel) cubeIndex |= 128;

            /* Cube is entirely in/out of the surface */
            if (EdgeTable[cubeIndex] == 0)
                return;

            /* Find the vertices where the surface intersects the cube */
            if ((EdgeTable[cubeIndex] & 1) > 0)
                VertexBuffer[0] = VertexInterp(Cell.P[0], Cell.P[1], Cell.Density[0], Cell.Density[1]);

            if ((EdgeTable[cubeIndex] & 2) > 0)
                VertexBuffer[1] = VertexInterp(Cell.P[1], Cell.P[2], Cell.Density[1], Cell.Density[2]);

            if ((EdgeTable[cubeIndex] & 4) > 0)
                VertexBuffer[2] = VertexInterp(Cell.P[2], Cell.P[3], Cell.Density[2], Cell.Density[3]);

            if ((EdgeTable[cubeIndex] & 8) > 0)
                VertexBuffer[3] = VertexInterp(Cell.P[3], Cell.P[0], Cell.Density[3], Cell.Density[0]);

            if ((EdgeTable[cubeIndex] & 16) > 0)
                VertexBuffer[4] = VertexInterp(Cell.P[4], Cell.P[5], Cell.Density[4], Cell.Density[5]);

            if ((EdgeTable[cubeIndex] & 32) > 0)
                VertexBuffer[5] = VertexInterp(Cell.P[5], Cell.P[6], Cell.Density[5], Cell.Density[6]);

            if ((EdgeTable[cubeIndex] & 64) > 0)
                VertexBuffer[6] = VertexInterp(Cell.P[6], Cell.P[7], Cell.Density[6], Cell.Density[7]);

            if ((EdgeTable[cubeIndex] & 128) > 0)
                VertexBuffer[7] = VertexInterp(Cell.P[7], Cell.P[4], Cell.Density[7], Cell.Density[4]);

            if ((EdgeTable[cubeIndex] & 256) > 0)
                VertexBuffer[8] = VertexInterp(Cell.P[0], Cell.P[4], Cell.Density[0], Cell.Density[4]);

            if ((EdgeTable[cubeIndex] & 512) > 0)
                VertexBuffer[9] = VertexInterp( Cell.P[1], Cell.P[5], Cell.Density[1], Cell.Density[5]);

            if ((EdgeTable[cubeIndex] & 1024) > 0)
                VertexBuffer[10] = VertexInterp(Cell.P[2], Cell.P[6], Cell.Density[2], Cell.Density[6]);

            if ((EdgeTable[cubeIndex] & 2048) > 0)
                VertexBuffer[11] = VertexInterp(Cell.P[3], Cell.P[7], Cell.Density[3], Cell.Density[7]);

            /* Create the triangle */
            for (var i = 0; TriTable[cubeIndex, i] != -1; i += 3)
            {
                TriangleBuffer[TriangleCount].Vertices[0] = VertexBuffer[TriTable[cubeIndex, i + 0]];
                TriangleBuffer[TriangleCount].Vertices[1] = VertexBuffer[TriTable[cubeIndex, i + 1]];
                TriangleBuffer[TriangleCount].Vertices[2] = VertexBuffer[TriTable[cubeIndex, i + 2]];
                TriangleCount++;
            }
        }


        private static Vector3 VertexInterp(Vector3 v1, Vector3 v2, float valp1, float valp2)
        {
            float t = (IsoLevel - valp1) / (valp2 - valp1);
            return (v1 + t * (v2 - v1));
        }


        public static void Build(ref NativeVertexData Data, ref Vector4 TemplateColor, ref Triangle[] TriangleBuffer,
            ref int TriangleCount, ref bool IsWater, ref bool IsRiverConstant)
        {
            for (uint i = 0; i < TriangleCount; i++)
            {
                if (IsWater && ShouldClip(ref TriangleBuffer[i], ref IsRiverConstant)) continue;
                Data.Indices.Add((uint)Data.Vertices.Count + 0);
                Data.Indices.Add((uint)Data.Vertices.Count + 1);
                Data.Indices.Add((uint)Data.Vertices.Count + 2);

                Data.Vertices.Add(TriangleBuffer[i].Vertices[0]);
                Data.Vertices.Add(TriangleBuffer[i].Vertices[1]);
                Data.Vertices.Add(TriangleBuffer[i].Vertices[2]);

                var normal = Vector3.Cross(TriangleBuffer[i].Vertices[1] - TriangleBuffer[i].Vertices[0],
                    TriangleBuffer[i].Vertices[2] - TriangleBuffer[i].Vertices[0]).Normalized();
                Data.Normals.Add(normal);
                Data.Normals.Add(normal);
                Data.Normals.Add(normal);

                Data.Colors.Add(TemplateColor);
                Data.Colors.Add(TemplateColor);
                Data.Colors.Add(TemplateColor);
            }
        }

        private static bool ShouldClip(ref Triangle Triangle, ref bool IsRiverConstant)
        {
            const float oceanClipDistance = (BiomePool.SeaLevel - 2) * Chunk.BlockSize;
            var isBelowOcean = Triangle.Vertices[0].Y < oceanClipDistance ||
                               Triangle.Vertices[1].Y < oceanClipDistance ||
                               Triangle.Vertices[2].Y < oceanClipDistance;

            const float maxRiverClipDistance = (BiomePool.RiverWaterLevel - 2) * Chunk.BlockSize;
            const float minRiverClipDistance = (BiomePool.RiverFloorLevel - 2) * Chunk.BlockSize;
            var isInRiverClipZone = Triangle.Vertices[0].Y > minRiverClipDistance &&
                                    Triangle.Vertices[1].Y > minRiverClipDistance &&
                                    Triangle.Vertices[2].Y > minRiverClipDistance;
            var isWithinRiver = Triangle.Vertices[0].Y < maxRiverClipDistance &&
                                Triangle.Vertices[1].Y < maxRiverClipDistance &&
                                Triangle.Vertices[2].Y < maxRiverClipDistance && isInRiverClipZone;
            return isWithinRiver && IsRiverConstant || isBelowOcean;
        }

        #region Tables

        private static readonly int[] EdgeTable = new int[256]
        {
            0x0, 0x109, 0x203, 0x30a, 0x406, 0x50f, 0x605, 0x70c,
            0x80c, 0x905, 0xa0f, 0xb06, 0xc0a, 0xd03, 0xe09, 0xf00,
            0x190, 0x99, 0x393, 0x29a, 0x596, 0x49f, 0x795, 0x69c,
            0x99c, 0x895, 0xb9f, 0xa96, 0xd9a, 0xc93, 0xf99, 0xe90,
            0x230, 0x339, 0x33, 0x13a, 0x636, 0x73f, 0x435, 0x53c,
            0xa3c, 0xb35, 0x83f, 0x936, 0xe3a, 0xf33, 0xc39, 0xd30,
            0x3a0, 0x2a9, 0x1a3, 0xaa, 0x7a6, 0x6af, 0x5a5, 0x4ac,
            0xbac, 0xaa5, 0x9af, 0x8a6, 0xfaa, 0xea3, 0xda9, 0xca0,
            0x460, 0x569, 0x663, 0x76a, 0x66, 0x16f, 0x265, 0x36c,
            0xc6c, 0xd65, 0xe6f, 0xf66, 0x86a, 0x963, 0xa69, 0xb60,
            0x5f0, 0x4f9, 0x7f3, 0x6fa, 0x1f6, 0xff, 0x3f5, 0x2fc,
            0xdfc, 0xcf5, 0xfff, 0xef6, 0x9fa, 0x8f3, 0xbf9, 0xaf0,
            0x650, 0x759, 0x453, 0x55a, 0x256, 0x35f, 0x55, 0x15c,
            0xe5c, 0xf55, 0xc5f, 0xd56, 0xa5a, 0xb53, 0x859, 0x950,
            0x7c0, 0x6c9, 0x5c3, 0x4ca, 0x3c6, 0x2cf, 0x1c5, 0xcc,
            0xfcc, 0xec5, 0xdcf, 0xcc6, 0xbca, 0xac3, 0x9c9, 0x8c0,
            0x8c0, 0x9c9, 0xac3, 0xbca, 0xcc6, 0xdcf, 0xec5, 0xfcc,
            0xcc, 0x1c5, 0x2cf, 0x3c6, 0x4ca, 0x5c3, 0x6c9, 0x7c0,
            0x950, 0x859, 0xb53, 0xa5a, 0xd56, 0xc5f, 0xf55, 0xe5c,
            0x15c, 0x55, 0x35f, 0x256, 0x55a, 0x453, 0x759, 0x650,
            0xaf0, 0xbf9, 0x8f3, 0x9fa, 0xef6, 0xfff, 0xcf5, 0xdfc,
            0x2fc, 0x3f5, 0xff, 0x1f6, 0x6fa, 0x7f3, 0x4f9, 0x5f0,
            0xb60, 0xa69, 0x963, 0x86a, 0xf66, 0xe6f, 0xd65, 0xc6c,
            0x36c, 0x265, 0x16f, 0x66, 0x76a, 0x663, 0x569, 0x460,
            0xca0, 0xda9, 0xea3, 0xfaa, 0x8a6, 0x9af, 0xaa5, 0xbac,
            0x4ac, 0x5a5, 0x6af, 0x7a6, 0xaa, 0x1a3, 0x2a9, 0x3a0,
            0xd30, 0xc39, 0xf33, 0xe3a, 0x936, 0x83f, 0xb35, 0xa3c,
            0x53c, 0x435, 0x73f, 0x636, 0x13a, 0x33, 0x339, 0x230,
            0xe90, 0xf99, 0xc93, 0xd9a, 0xa96, 0xb9f, 0x895, 0x99c,
            0x69c, 0x795, 0x49f, 0x596, 0x29a, 0x393, 0x99, 0x190,
            0xf00, 0xe09, 0xd03, 0xc0a, 0xb06, 0xa0f, 0x905, 0x80c,
            0x70c, 0x605, 0x50f, 0x406, 0x30a, 0x203, 0x109, 0x0
        };

        private static readonly int[,] TriTable = new int[256, 16]
        {
            { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 0, 1, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 1, 8, 3, 9, 8, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 0, 8, 3, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 9, 2, 10, 0, 2, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 2, 8, 3, 2, 10, 8, 10, 9, 8, -1, -1, -1, -1, -1, -1, -1 },
            { 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 0, 11, 2, 8, 11, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 1, 9, 0, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 1, 11, 2, 1, 9, 11, 9, 8, 11, -1, -1, -1, -1, -1, -1, -1 },
            { 3, 10, 1, 11, 10, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 0, 10, 1, 0, 8, 10, 8, 11, 10, -1, -1, -1, -1, -1, -1, -1 },
            { 3, 9, 0, 3, 11, 9, 11, 10, 9, -1, -1, -1, -1, -1, -1, -1 },
            { 9, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 4, 3, 0, 7, 3, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 0, 1, 9, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 4, 1, 9, 4, 7, 1, 7, 3, 1, -1, -1, -1, -1, -1, -1, -1 },
            { 1, 2, 10, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 3, 4, 7, 3, 0, 4, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1 },
            { 9, 2, 10, 9, 0, 2, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1 },
            { 2, 10, 9, 2, 9, 7, 2, 7, 3, 7, 9, 4, -1, -1, -1, -1 },
            { 8, 4, 7, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 11, 4, 7, 11, 2, 4, 2, 0, 4, -1, -1, -1, -1, -1, -1, -1 },
            { 9, 0, 1, 8, 4, 7, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1 },
            { 4, 7, 11, 9, 4, 11, 9, 11, 2, 9, 2, 1, -1, -1, -1, -1 },
            { 3, 10, 1, 3, 11, 10, 7, 8, 4, -1, -1, -1, -1, -1, -1, -1 },
            { 1, 11, 10, 1, 4, 11, 1, 0, 4, 7, 11, 4, -1, -1, -1, -1 },
            { 4, 7, 8, 9, 0, 11, 9, 11, 10, 11, 0, 3, -1, -1, -1, -1 },
            { 4, 7, 11, 4, 11, 9, 9, 11, 10, -1, -1, -1, -1, -1, -1, -1 },
            { 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 9, 5, 4, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 0, 5, 4, 1, 5, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 8, 5, 4, 8, 3, 5, 3, 1, 5, -1, -1, -1, -1, -1, -1, -1 },
            { 1, 2, 10, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 3, 0, 8, 1, 2, 10, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1 },
            { 5, 2, 10, 5, 4, 2, 4, 0, 2, -1, -1, -1, -1, -1, -1, -1 },
            { 2, 10, 5, 3, 2, 5, 3, 5, 4, 3, 4, 8, -1, -1, -1, -1 },
            { 9, 5, 4, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 0, 11, 2, 0, 8, 11, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1 },
            { 0, 5, 4, 0, 1, 5, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1 },
            { 2, 1, 5, 2, 5, 8, 2, 8, 11, 4, 8, 5, -1, -1, -1, -1 },
            { 10, 3, 11, 10, 1, 3, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1 },
            { 4, 9, 5, 0, 8, 1, 8, 10, 1, 8, 11, 10, -1, -1, -1, -1 },
            { 5, 4, 0, 5, 0, 11, 5, 11, 10, 11, 0, 3, -1, -1, -1, -1 },
            { 5, 4, 8, 5, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1 },
            { 9, 7, 8, 5, 7, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 9, 3, 0, 9, 5, 3, 5, 7, 3, -1, -1, -1, -1, -1, -1, -1 },
            { 0, 7, 8, 0, 1, 7, 1, 5, 7, -1, -1, -1, -1, -1, -1, -1 },
            { 1, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 9, 7, 8, 9, 5, 7, 10, 1, 2, -1, -1, -1, -1, -1, -1, -1 },
            { 10, 1, 2, 9, 5, 0, 5, 3, 0, 5, 7, 3, -1, -1, -1, -1 },
            { 8, 0, 2, 8, 2, 5, 8, 5, 7, 10, 5, 2, -1, -1, -1, -1 },
            { 2, 10, 5, 2, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1 },
            { 7, 9, 5, 7, 8, 9, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1 },
            { 9, 5, 7, 9, 7, 2, 9, 2, 0, 2, 7, 11, -1, -1, -1, -1 },
            { 2, 3, 11, 0, 1, 8, 1, 7, 8, 1, 5, 7, -1, -1, -1, -1 },
            { 11, 2, 1, 11, 1, 7, 7, 1, 5, -1, -1, -1, -1, -1, -1, -1 },
            { 9, 5, 8, 8, 5, 7, 10, 1, 3, 10, 3, 11, -1, -1, -1, -1 },
            { 5, 7, 0, 5, 0, 9, 7, 11, 0, 1, 0, 10, 11, 10, 0, -1 },
            { 11, 10, 0, 11, 0, 3, 10, 5, 0, 8, 0, 7, 5, 7, 0, -1 },
            { 11, 10, 5, 7, 11, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 0, 8, 3, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 9, 0, 1, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 1, 8, 3, 1, 9, 8, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1 },
            { 1, 6, 5, 2, 6, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 1, 6, 5, 1, 2, 6, 3, 0, 8, -1, -1, -1, -1, -1, -1, -1 },
            { 9, 6, 5, 9, 0, 6, 0, 2, 6, -1, -1, -1, -1, -1, -1, -1 },
            { 5, 9, 8, 5, 8, 2, 5, 2, 6, 3, 2, 8, -1, -1, -1, -1 },
            { 2, 3, 11, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 11, 0, 8, 11, 2, 0, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1 },
            { 0, 1, 9, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1 },
            { 5, 10, 6, 1, 9, 2, 9, 11, 2, 9, 8, 11, -1, -1, -1, -1 },
            { 6, 3, 11, 6, 5, 3, 5, 1, 3, -1, -1, -1, -1, -1, -1, -1 },
            { 0, 8, 11, 0, 11, 5, 0, 5, 1, 5, 11, 6, -1, -1, -1, -1 },
            { 3, 11, 6, 0, 3, 6, 0, 6, 5, 0, 5, 9, -1, -1, -1, -1 },
            { 6, 5, 9, 6, 9, 11, 11, 9, 8, -1, -1, -1, -1, -1, -1, -1 },
            { 5, 10, 6, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 4, 3, 0, 4, 7, 3, 6, 5, 10, -1, -1, -1, -1, -1, -1, -1 },
            { 1, 9, 0, 5, 10, 6, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1 },
            { 10, 6, 5, 1, 9, 7, 1, 7, 3, 7, 9, 4, -1, -1, -1, -1 },
            { 6, 1, 2, 6, 5, 1, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1 },
            { 1, 2, 5, 5, 2, 6, 3, 0, 4, 3, 4, 7, -1, -1, -1, -1 },
            { 8, 4, 7, 9, 0, 5, 0, 6, 5, 0, 2, 6, -1, -1, -1, -1 },
            { 7, 3, 9, 7, 9, 4, 3, 2, 9, 5, 9, 6, 2, 6, 9, -1 },
            { 3, 11, 2, 7, 8, 4, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1 },
            { 5, 10, 6, 4, 7, 2, 4, 2, 0, 2, 7, 11, -1, -1, -1, -1 },
            { 0, 1, 9, 4, 7, 8, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1 },
            { 9, 2, 1, 9, 11, 2, 9, 4, 11, 7, 11, 4, 5, 10, 6, -1 },
            { 8, 4, 7, 3, 11, 5, 3, 5, 1, 5, 11, 6, -1, -1, -1, -1 },
            { 5, 1, 11, 5, 11, 6, 1, 0, 11, 7, 11, 4, 0, 4, 11, -1 },
            { 0, 5, 9, 0, 6, 5, 0, 3, 6, 11, 6, 3, 8, 4, 7, -1 },
            { 6, 5, 9, 6, 9, 11, 4, 7, 9, 7, 11, 9, -1, -1, -1, -1 },
            { 10, 4, 9, 6, 4, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 4, 10, 6, 4, 9, 10, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1 },
            { 10, 0, 1, 10, 6, 0, 6, 4, 0, -1, -1, -1, -1, -1, -1, -1 },
            { 8, 3, 1, 8, 1, 6, 8, 6, 4, 6, 1, 10, -1, -1, -1, -1 },
            { 1, 4, 9, 1, 2, 4, 2, 6, 4, -1, -1, -1, -1, -1, -1, -1 },
            { 3, 0, 8, 1, 2, 9, 2, 4, 9, 2, 6, 4, -1, -1, -1, -1 },
            { 0, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 8, 3, 2, 8, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1 },
            { 10, 4, 9, 10, 6, 4, 11, 2, 3, -1, -1, -1, -1, -1, -1, -1 },
            { 0, 8, 2, 2, 8, 11, 4, 9, 10, 4, 10, 6, -1, -1, -1, -1 },
            { 3, 11, 2, 0, 1, 6, 0, 6, 4, 6, 1, 10, -1, -1, -1, -1 },
            { 6, 4, 1, 6, 1, 10, 4, 8, 1, 2, 1, 11, 8, 11, 1, -1 },
            { 9, 6, 4, 9, 3, 6, 9, 1, 3, 11, 6, 3, -1, -1, -1, -1 },
            { 8, 11, 1, 8, 1, 0, 11, 6, 1, 9, 1, 4, 6, 4, 1, -1 },
            { 3, 11, 6, 3, 6, 0, 0, 6, 4, -1, -1, -1, -1, -1, -1, -1 },
            { 6, 4, 8, 11, 6, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 7, 10, 6, 7, 8, 10, 8, 9, 10, -1, -1, -1, -1, -1, -1, -1 },
            { 0, 7, 3, 0, 10, 7, 0, 9, 10, 6, 7, 10, -1, -1, -1, -1 },
            { 10, 6, 7, 1, 10, 7, 1, 7, 8, 1, 8, 0, -1, -1, -1, -1 },
            { 10, 6, 7, 10, 7, 1, 1, 7, 3, -1, -1, -1, -1, -1, -1, -1 },
            { 1, 2, 6, 1, 6, 8, 1, 8, 9, 8, 6, 7, -1, -1, -1, -1 },
            { 2, 6, 9, 2, 9, 1, 6, 7, 9, 0, 9, 3, 7, 3, 9, -1 },
            { 7, 8, 0, 7, 0, 6, 6, 0, 2, -1, -1, -1, -1, -1, -1, -1 },
            { 7, 3, 2, 6, 7, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 2, 3, 11, 10, 6, 8, 10, 8, 9, 8, 6, 7, -1, -1, -1, -1 },
            { 2, 0, 7, 2, 7, 11, 0, 9, 7, 6, 7, 10, 9, 10, 7, -1 },
            { 1, 8, 0, 1, 7, 8, 1, 10, 7, 6, 7, 10, 2, 3, 11, -1 },
            { 11, 2, 1, 11, 1, 7, 10, 6, 1, 6, 7, 1, -1, -1, -1, -1 },
            { 8, 9, 6, 8, 6, 7, 9, 1, 6, 11, 6, 3, 1, 3, 6, -1 },
            { 0, 9, 1, 11, 6, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 7, 8, 0, 7, 0, 6, 3, 11, 0, 11, 6, 0, -1, -1, -1, -1 },
            { 7, 11, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 3, 0, 8, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 0, 1, 9, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 8, 1, 9, 8, 3, 1, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1 },
            { 10, 1, 2, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 1, 2, 10, 3, 0, 8, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1 },
            { 2, 9, 0, 2, 10, 9, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1 },
            { 6, 11, 7, 2, 10, 3, 10, 8, 3, 10, 9, 8, -1, -1, -1, -1 },
            { 7, 2, 3, 6, 2, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 7, 0, 8, 7, 6, 0, 6, 2, 0, -1, -1, -1, -1, -1, -1, -1 },
            { 2, 7, 6, 2, 3, 7, 0, 1, 9, -1, -1, -1, -1, -1, -1, -1 },
            { 1, 6, 2, 1, 8, 6, 1, 9, 8, 8, 7, 6, -1, -1, -1, -1 },
            { 10, 7, 6, 10, 1, 7, 1, 3, 7, -1, -1, -1, -1, -1, -1, -1 },
            { 10, 7, 6, 1, 7, 10, 1, 8, 7, 1, 0, 8, -1, -1, -1, -1 },
            { 0, 3, 7, 0, 7, 10, 0, 10, 9, 6, 10, 7, -1, -1, -1, -1 },
            { 7, 6, 10, 7, 10, 8, 8, 10, 9, -1, -1, -1, -1, -1, -1, -1 },
            { 6, 8, 4, 11, 8, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 3, 6, 11, 3, 0, 6, 0, 4, 6, -1, -1, -1, -1, -1, -1, -1 },
            { 8, 6, 11, 8, 4, 6, 9, 0, 1, -1, -1, -1, -1, -1, -1, -1 },
            { 9, 4, 6, 9, 6, 3, 9, 3, 1, 11, 3, 6, -1, -1, -1, -1 },
            { 6, 8, 4, 6, 11, 8, 2, 10, 1, -1, -1, -1, -1, -1, -1, -1 },
            { 1, 2, 10, 3, 0, 11, 0, 6, 11, 0, 4, 6, -1, -1, -1, -1 },
            { 4, 11, 8, 4, 6, 11, 0, 2, 9, 2, 10, 9, -1, -1, -1, -1 },
            { 10, 9, 3, 10, 3, 2, 9, 4, 3, 11, 3, 6, 4, 6, 3, -1 },
            { 8, 2, 3, 8, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1 },
            { 0, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 1, 9, 0, 2, 3, 4, 2, 4, 6, 4, 3, 8, -1, -1, -1, -1 },
            { 1, 9, 4, 1, 4, 2, 2, 4, 6, -1, -1, -1, -1, -1, -1, -1 },
            { 8, 1, 3, 8, 6, 1, 8, 4, 6, 6, 10, 1, -1, -1, -1, -1 },
            { 10, 1, 0, 10, 0, 6, 6, 0, 4, -1, -1, -1, -1, -1, -1, -1 },
            { 4, 6, 3, 4, 3, 8, 6, 10, 3, 0, 3, 9, 10, 9, 3, -1 },
            { 10, 9, 4, 6, 10, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 4, 9, 5, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 0, 8, 3, 4, 9, 5, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1 },
            { 5, 0, 1, 5, 4, 0, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1 },
            { 11, 7, 6, 8, 3, 4, 3, 5, 4, 3, 1, 5, -1, -1, -1, -1 },
            { 9, 5, 4, 10, 1, 2, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1 },
            { 6, 11, 7, 1, 2, 10, 0, 8, 3, 4, 9, 5, -1, -1, -1, -1 },
            { 7, 6, 11, 5, 4, 10, 4, 2, 10, 4, 0, 2, -1, -1, -1, -1 },
            { 3, 4, 8, 3, 5, 4, 3, 2, 5, 10, 5, 2, 11, 7, 6, -1 },
            { 7, 2, 3, 7, 6, 2, 5, 4, 9, -1, -1, -1, -1, -1, -1, -1 },
            { 9, 5, 4, 0, 8, 6, 0, 6, 2, 6, 8, 7, -1, -1, -1, -1 },
            { 3, 6, 2, 3, 7, 6, 1, 5, 0, 5, 4, 0, -1, -1, -1, -1 },
            { 6, 2, 8, 6, 8, 7, 2, 1, 8, 4, 8, 5, 1, 5, 8, -1 },
            { 9, 5, 4, 10, 1, 6, 1, 7, 6, 1, 3, 7, -1, -1, -1, -1 },
            { 1, 6, 10, 1, 7, 6, 1, 0, 7, 8, 7, 0, 9, 5, 4, -1 },
            { 4, 0, 10, 4, 10, 5, 0, 3, 10, 6, 10, 7, 3, 7, 10, -1 },
            { 7, 6, 10, 7, 10, 8, 5, 4, 10, 4, 8, 10, -1, -1, -1, -1 },
            { 6, 9, 5, 6, 11, 9, 11, 8, 9, -1, -1, -1, -1, -1, -1, -1 },
            { 3, 6, 11, 0, 6, 3, 0, 5, 6, 0, 9, 5, -1, -1, -1, -1 },
            { 0, 11, 8, 0, 5, 11, 0, 1, 5, 5, 6, 11, -1, -1, -1, -1 },
            { 6, 11, 3, 6, 3, 5, 5, 3, 1, -1, -1, -1, -1, -1, -1, -1 },
            { 1, 2, 10, 9, 5, 11, 9, 11, 8, 11, 5, 6, -1, -1, -1, -1 },
            { 0, 11, 3, 0, 6, 11, 0, 9, 6, 5, 6, 9, 1, 2, 10, -1 },
            { 11, 8, 5, 11, 5, 6, 8, 0, 5, 10, 5, 2, 0, 2, 5, -1 },
            { 6, 11, 3, 6, 3, 5, 2, 10, 3, 10, 5, 3, -1, -1, -1, -1 },
            { 5, 8, 9, 5, 2, 8, 5, 6, 2, 3, 8, 2, -1, -1, -1, -1 },
            { 9, 5, 6, 9, 6, 0, 0, 6, 2, -1, -1, -1, -1, -1, -1, -1 },
            { 1, 5, 8, 1, 8, 0, 5, 6, 8, 3, 8, 2, 6, 2, 8, -1 },
            { 1, 5, 6, 2, 1, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 1, 3, 6, 1, 6, 10, 3, 8, 6, 5, 6, 9, 8, 9, 6, -1 },
            { 10, 1, 0, 10, 0, 6, 9, 5, 0, 5, 6, 0, -1, -1, -1, -1 },
            { 0, 3, 8, 5, 6, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 10, 5, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 11, 5, 10, 7, 5, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 11, 5, 10, 11, 7, 5, 8, 3, 0, -1, -1, -1, -1, -1, -1, -1 },
            { 5, 11, 7, 5, 10, 11, 1, 9, 0, -1, -1, -1, -1, -1, -1, -1 },
            { 10, 7, 5, 10, 11, 7, 9, 8, 1, 8, 3, 1, -1, -1, -1, -1 },
            { 11, 1, 2, 11, 7, 1, 7, 5, 1, -1, -1, -1, -1, -1, -1, -1 },
            { 0, 8, 3, 1, 2, 7, 1, 7, 5, 7, 2, 11, -1, -1, -1, -1 },
            { 9, 7, 5, 9, 2, 7, 9, 0, 2, 2, 11, 7, -1, -1, -1, -1 },
            { 7, 5, 2, 7, 2, 11, 5, 9, 2, 3, 2, 8, 9, 8, 2, -1 },
            { 2, 5, 10, 2, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1 },
            { 8, 2, 0, 8, 5, 2, 8, 7, 5, 10, 2, 5, -1, -1, -1, -1 },
            { 9, 0, 1, 5, 10, 3, 5, 3, 7, 3, 10, 2, -1, -1, -1, -1 },
            { 9, 8, 2, 9, 2, 1, 8, 7, 2, 10, 2, 5, 7, 5, 2, -1 },
            { 1, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 0, 8, 7, 0, 7, 1, 1, 7, 5, -1, -1, -1, -1, -1, -1, -1 },
            { 9, 0, 3, 9, 3, 5, 5, 3, 7, -1, -1, -1, -1, -1, -1, -1 },
            { 9, 8, 7, 5, 9, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 5, 8, 4, 5, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1 },
            { 5, 0, 4, 5, 11, 0, 5, 10, 11, 11, 3, 0, -1, -1, -1, -1 },
            { 0, 1, 9, 8, 4, 10, 8, 10, 11, 10, 4, 5, -1, -1, -1, -1 },
            { 10, 11, 4, 10, 4, 5, 11, 3, 4, 9, 4, 1, 3, 1, 4, -1 },
            { 2, 5, 1, 2, 8, 5, 2, 11, 8, 4, 5, 8, -1, -1, -1, -1 },
            { 0, 4, 11, 0, 11, 3, 4, 5, 11, 2, 11, 1, 5, 1, 11, -1 },
            { 0, 2, 5, 0, 5, 9, 2, 11, 5, 4, 5, 8, 11, 8, 5, -1 },
            { 9, 4, 5, 2, 11, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 2, 5, 10, 3, 5, 2, 3, 4, 5, 3, 8, 4, -1, -1, -1, -1 },
            { 5, 10, 2, 5, 2, 4, 4, 2, 0, -1, -1, -1, -1, -1, -1, -1 },
            { 3, 10, 2, 3, 5, 10, 3, 8, 5, 4, 5, 8, 0, 1, 9, -1 },
            { 5, 10, 2, 5, 2, 4, 1, 9, 2, 9, 4, 2, -1, -1, -1, -1 },
            { 8, 4, 5, 8, 5, 3, 3, 5, 1, -1, -1, -1, -1, -1, -1, -1 },
            { 0, 4, 5, 1, 0, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 8, 4, 5, 8, 5, 3, 9, 0, 5, 0, 3, 5, -1, -1, -1, -1 },
            { 9, 4, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 4, 11, 7, 4, 9, 11, 9, 10, 11, -1, -1, -1, -1, -1, -1, -1 },
            { 0, 8, 3, 4, 9, 7, 9, 11, 7, 9, 10, 11, -1, -1, -1, -1 },
            { 1, 10, 11, 1, 11, 4, 1, 4, 0, 7, 4, 11, -1, -1, -1, -1 },
            { 3, 1, 4, 3, 4, 8, 1, 10, 4, 7, 4, 11, 10, 11, 4, -1 },
            { 4, 11, 7, 9, 11, 4, 9, 2, 11, 9, 1, 2, -1, -1, -1, -1 },
            { 9, 7, 4, 9, 11, 7, 9, 1, 11, 2, 11, 1, 0, 8, 3, -1 },
            { 11, 7, 4, 11, 4, 2, 2, 4, 0, -1, -1, -1, -1, -1, -1, -1 },
            { 11, 7, 4, 11, 4, 2, 8, 3, 4, 3, 2, 4, -1, -1, -1, -1 },
            { 2, 9, 10, 2, 7, 9, 2, 3, 7, 7, 4, 9, -1, -1, -1, -1 },
            { 9, 10, 7, 9, 7, 4, 10, 2, 7, 8, 7, 0, 2, 0, 7, -1 },
            { 3, 7, 10, 3, 10, 2, 7, 4, 10, 1, 10, 0, 4, 0, 10, -1 },
            { 1, 10, 2, 8, 7, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 4, 9, 1, 4, 1, 7, 7, 1, 3, -1, -1, -1, -1, -1, -1, -1 },
            { 4, 9, 1, 4, 1, 7, 0, 8, 1, 8, 7, 1, -1, -1, -1, -1 },
            { 4, 0, 3, 7, 4, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 4, 8, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 9, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 3, 0, 9, 3, 9, 11, 11, 9, 10, -1, -1, -1, -1, -1, -1, -1 },
            { 0, 1, 10, 0, 10, 8, 8, 10, 11, -1, -1, -1, -1, -1, -1, -1 },
            { 3, 1, 10, 11, 3, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 1, 2, 11, 1, 11, 9, 9, 11, 8, -1, -1, -1, -1, -1, -1, -1 },
            { 3, 0, 9, 3, 9, 11, 1, 2, 9, 2, 11, 9, -1, -1, -1, -1 },
            { 0, 2, 11, 8, 0, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 3, 2, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 2, 3, 8, 2, 8, 10, 10, 8, 9, -1, -1, -1, -1, -1, -1, -1 },
            { 9, 10, 2, 0, 9, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 2, 3, 8, 2, 8, 10, 0, 1, 8, 1, 10, 8, -1, -1, -1, -1 },
            { 1, 10, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 1, 3, 8, 9, 1, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 0, 9, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { 0, 3, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }
        };

        private static readonly byte[] TriangulationTable = new byte[]
        { 
			 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  0,  8,  3, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  0,  1,  9, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  1,  8,  3,  9,  8,  1, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  1,  2, 10, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  0,  8,  3,  1,  2, 10, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  9,  2, 10,  0,  2,  9, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  2,  8,  3,  2, 10,  8, 10,  9,  8, 99, 99, 99, 99, 99, 99, 99,
			  3, 11,  2, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  0, 11,  2,  8, 11,  0, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  1,  9,  0,  2,  3, 11, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  1, 11,  2,  1,  9, 11,  9,  8, 11, 99, 99, 99, 99, 99, 99, 99,
			  3, 10,  1, 11, 10,  3, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  0, 10,  1,  0,  8, 10,  8, 11, 10, 99, 99, 99, 99, 99, 99, 99,
			  3,  9,  0,  3, 11,  9, 11, 10,  9, 99, 99, 99, 99, 99, 99, 99,
			  9,  8, 10, 10,  8, 11, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  4,  7,  8, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  4,  3,  0,  7,  3,  4, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  0,  1,  9,  8,  4,  7, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  4,  1,  9,  4,  7,  1,  7,  3,  1, 99, 99, 99, 99, 99, 99, 99,
			  1,  2, 10,  8,  4,  7, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  3,  4,  7,  3,  0,  4,  1,  2, 10, 99, 99, 99, 99, 99, 99, 99,
			  9,  2, 10,  9,  0,  2,  8,  4,  7, 99, 99, 99, 99, 99, 99, 99,
			  2, 10,  9,  2,  9,  7,  2,  7,  3,  7,  9,  4, 99, 99, 99, 99,
			  8,  4,  7,  3, 11,  2, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			 11,  4,  7, 11,  2,  4,  2,  0,  4, 99, 99, 99, 99, 99, 99, 99,
			  9,  0,  1,  8,  4,  7,  2,  3, 11, 99, 99, 99, 99, 99, 99, 99,
			  4,  7, 11,  9,  4, 11,  9, 11,  2,  9,  2,  1, 99, 99, 99, 99,
			  3, 10,  1,  3, 11, 10,  7,  8,  4, 99, 99, 99, 99, 99, 99, 99,
			  1, 11, 10,  1,  4, 11,  1,  0,  4,  7, 11,  4, 99, 99, 99, 99,
			  4,  7,  8,  9,  0, 11,  9, 11, 10, 11,  0,  3, 99, 99, 99, 99,
			  4,  7, 11,  4, 11,  9,  9, 11, 10, 99, 99, 99, 99, 99, 99, 99,
			  9,  5,  4, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  9,  5,  4,  0,  8,  3, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  0,  5,  4,  1,  5,  0, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  8,  5,  4,  8,  3,  5,  3,  1,  5, 99, 99, 99, 99, 99, 99, 99,
			  1,  2, 10,  9,  5,  4, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  3,  0,  8,  1,  2, 10,  4,  9,  5, 99, 99, 99, 99, 99, 99, 99,
			  5,  2, 10,  5,  4,  2,  4,  0,  2, 99, 99, 99, 99, 99, 99, 99,
			  2, 10,  5,  3,  2,  5,  3,  5,  4,  3,  4,  8, 99, 99, 99, 99,
			  9,  5,  4,  2,  3, 11, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  0, 11,  2,  0,  8, 11,  4,  9,  5, 99, 99, 99, 99, 99, 99, 99,
			  0,  5,  4,  0,  1,  5,  2,  3, 11, 99, 99, 99, 99, 99, 99, 99,
			  2,  1,  5,  2,  5,  8,  2,  8, 11,  4,  8,  5, 99, 99, 99, 99,
			 10,  3, 11, 10,  1,  3,  9,  5,  4, 99, 99, 99, 99, 99, 99, 99,
			  4,  9,  5,  0,  8,  1,  8, 10,  1,  8, 11, 10, 99, 99, 99, 99,
			  5,  4,  0,  5,  0, 11,  5, 11, 10, 11,  0,  3, 99, 99, 99, 99,
			  5,  4,  8,  5,  8, 10, 10,  8, 11, 99, 99, 99, 99, 99, 99, 99,
			  9,  7,  8,  5,  7,  9, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  9,  3,  0,  9,  5,  3,  5,  7,  3, 99, 99, 99, 99, 99, 99, 99,
			  0,  7,  8,  0,  1,  7,  1,  5,  7, 99, 99, 99, 99, 99, 99, 99,
			  1,  5,  3,  3,  5,  7, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  9,  7,  8,  9,  5,  7, 10,  1,  2, 99, 99, 99, 99, 99, 99, 99,
			 10,  1,  2,  9,  5,  0,  5,  3,  0,  5,  7,  3, 99, 99, 99, 99,
			  8,  0,  2,  8,  2,  5,  8,  5,  7, 10,  5,  2, 99, 99, 99, 99,
			  2, 10,  5,  2,  5,  3,  3,  5,  7, 99, 99, 99, 99, 99, 99, 99,
			  7,  9,  5,  7,  8,  9,  3, 11,  2, 99, 99, 99, 99, 99, 99, 99,
			  9,  5,  7,  9,  7,  2,  9,  2,  0,  2,  7, 11, 99, 99, 99, 99,
			  2,  3, 11,  0,  1,  8,  1,  7,  8,  1,  5,  7, 99, 99, 99, 99,
			 11,  2,  1, 11,  1,  7,  7,  1,  5, 99, 99, 99, 99, 99, 99, 99,
			  9,  5,  8,  8,  5,  7, 10,  1,  3, 10,  3, 11, 99, 99, 99, 99,
			  5,  7,  0,  5,  0,  9,  7, 11,  0,  1,  0, 10, 11, 10,  0, 99,
			 11, 10,  0, 11,  0,  3, 10,  5,  0,  8,  0,  7,  5,  7,  0, 99,
			 11, 10,  5,  7, 11,  5, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			 10,  6,  5, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  0,  8,  3,  5, 10,  6, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  9,  0,  1,  5, 10,  6, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  1,  8,  3,  1,  9,  8,  5, 10,  6, 99, 99, 99, 99, 99, 99, 99,
			  1,  6,  5,  2,  6,  1, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  1,  6,  5,  1,  2,  6,  3,  0,  8, 99, 99, 99, 99, 99, 99, 99,
			  9,  6,  5,  9,  0,  6,  0,  2,  6, 99, 99, 99, 99, 99, 99, 99,
			  5,  9,  8,  5,  8,  2,  5,  2,  6,  3,  2,  8, 99, 99, 99, 99,
			  2,  3, 11, 10,  6,  5, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			 11,  0,  8, 11,  2,  0, 10,  6,  5, 99, 99, 99, 99, 99, 99, 99,
			  0,  1,  9,  2,  3, 11,  5, 10,  6, 99, 99, 99, 99, 99, 99, 99,
			  5, 10,  6,  1,  9,  2,  9, 11,  2,  9,  8, 11, 99, 99, 99, 99,
			  6,  3, 11,  6,  5,  3,  5,  1,  3, 99, 99, 99, 99, 99, 99, 99,
			  0,  8, 11,  0, 11,  5,  0,  5,  1,  5, 11,  6, 99, 99, 99, 99,
			  3, 11,  6,  0,  3,  6,  0,  6,  5,  0,  5,  9, 99, 99, 99, 99,
			  6,  5,  9,  6,  9, 11, 11,  9,  8, 99, 99, 99, 99, 99, 99, 99,
			  5, 10,  6,  4,  7,  8, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  4,  3,  0,  4,  7,  3,  6,  5, 10, 99, 99, 99, 99, 99, 99, 99,
			  1,  9,  0,  5, 10,  6,  8,  4,  7, 99, 99, 99, 99, 99, 99, 99,
			 10,  6,  5,  1,  9,  7,  1,  7,  3,  7,  9,  4, 99, 99, 99, 99,
			  6,  1,  2,  6,  5,  1,  4,  7,  8, 99, 99, 99, 99, 99, 99, 99,
			  1,  2,  5,  5,  2,  6,  3,  0,  4,  3,  4,  7, 99, 99, 99, 99,
			  8,  4,  7,  9,  0,  5,  0,  6,  5,  0,  2,  6, 99, 99, 99, 99,
			  7,  3,  9,  7,  9,  4,  3,  2,  9,  5,  9,  6,  2,  6,  9, 99,
			  3, 11,  2,  7,  8,  4, 10,  6,  5, 99, 99, 99, 99, 99, 99, 99,
			  5, 10,  6,  4,  7,  2,  4,  2,  0,  2,  7, 11, 99, 99, 99, 99,
			  0,  1,  9,  4,  7,  8,  2,  3, 11,  5, 10,  6, 99, 99, 99, 99,
			  9,  2,  1,  9, 11,  2,  9,  4, 11,  7, 11,  4,  5, 10,  6, 99,
			  8,  4,  7,  3, 11,  5,  3,  5,  1,  5, 11,  6, 99, 99, 99, 99,
			  5,  1, 11,  5, 11,  6,  1,  0, 11,  7, 11,  4,  0,  4, 11, 99,
			  0,  5,  9,  0,  6,  5,  0,  3,  6, 11,  6,  3,  8,  4,  7, 99,
			  6,  5,  9,  6,  9, 11,  4,  7,  9,  7, 11,  9, 99, 99, 99, 99,
			 10,  4,  9,  6,  4, 10, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  4, 10,  6,  4,  9, 10,  0,  8,  3, 99, 99, 99, 99, 99, 99, 99,
			 10,  0,  1, 10,  6,  0,  6,  4,  0, 99, 99, 99, 99, 99, 99, 99,
			  8,  3,  1,  8,  1,  6,  8,  6,  4,  6,  1, 10, 99, 99, 99, 99,
			  1,  4,  9,  1,  2,  4,  2,  6,  4, 99, 99, 99, 99, 99, 99, 99,
			  3,  0,  8,  1,  2,  9,  2,  4,  9,  2,  6,  4, 99, 99, 99, 99,
			  0,  2,  4,  4,  2,  6, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  8,  3,  2,  8,  2,  4,  4,  2,  6, 99, 99, 99, 99, 99, 99, 99,
			 10,  4,  9, 10,  6,  4, 11,  2,  3, 99, 99, 99, 99, 99, 99, 99,
			  0,  8,  2,  2,  8, 11,  4,  9, 10,  4, 10,  6, 99, 99, 99, 99,
			  3, 11,  2,  0,  1,  6,  0,  6,  4,  6,  1, 10, 99, 99, 99, 99,
			  6,  4,  1,  6,  1, 10,  4,  8,  1,  2,  1, 11,  8, 11,  1, 99,
			  9,  6,  4,  9,  3,  6,  9,  1,  3, 11,  6,  3, 99, 99, 99, 99,
			  8, 11,  1,  8,  1,  0, 11,  6,  1,  9,  1,  4,  6,  4,  1, 99,
			  3, 11,  6,  3,  6,  0,  0,  6,  4, 99, 99, 99, 99, 99, 99, 99,
			  6,  4,  8, 11,  6,  8, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  7, 10,  6,  7,  8, 10,  8,  9, 10, 99, 99, 99, 99, 99, 99, 99,
			  0,  7,  3,  0, 10,  7,  0,  9, 10,  6,  7, 10, 99, 99, 99, 99,
			 10,  6,  7,  1, 10,  7,  1,  7,  8,  1,  8,  0, 99, 99, 99, 99,
			 10,  6,  7, 10,  7,  1,  1,  7,  3, 99, 99, 99, 99, 99, 99, 99,
			  1,  2,  6,  1,  6,  8,  1,  8,  9,  8,  6,  7, 99, 99, 99, 99,
			  2,  6,  9,  2,  9,  1,  6,  7,  9,  0,  9,  3,  7,  3,  9, 99,
			  7,  8,  0,  7,  0,  6,  6,  0,  2, 99, 99, 99, 99, 99, 99, 99,
			  7,  3,  2,  6,  7,  2, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  2,  3, 11, 10,  6,  8, 10,  8,  9,  8,  6,  7, 99, 99, 99, 99,
			  2,  0,  7,  2,  7, 11,  0,  9,  7,  6,  7, 10,  9, 10,  7, 99,
			  1,  8,  0,  1,  7,  8,  1, 10,  7,  6,  7, 10,  2,  3, 11, 99,
			 11,  2,  1, 11,  1,  7, 10,  6,  1,  6,  7,  1, 99, 99, 99, 99,
			  8,  9,  6,  8,  6,  7,  9,  1,  6, 11,  6,  3,  1,  3,  6, 99,
			  0,  9,  1, 11,  6,  7, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  7,  8,  0,  7,  0,  6,  3, 11,  0, 11,  6,  0, 99, 99, 99, 99,
			  7, 11,  6, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  7,  6, 11, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  3,  0,  8, 11,  7,  6, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  0,  1,  9, 11,  7,  6, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  8,  1,  9,  8,  3,  1, 11,  7,  6, 99, 99, 99, 99, 99, 99, 99,
			 10,  1,  2,  6, 11,  7, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  1,  2, 10,  3,  0,  8,  6, 11,  7, 99, 99, 99, 99, 99, 99, 99,
			  2,  9,  0,  2, 10,  9,  6, 11,  7, 99, 99, 99, 99, 99, 99, 99,
			  6, 11,  7,  2, 10,  3, 10,  8,  3, 10,  9,  8, 99, 99, 99, 99,
			  7,  2,  3,  6,  2,  7, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  7,  0,  8,  7,  6,  0,  6,  2,  0, 99, 99, 99, 99, 99, 99, 99,
			  2,  7,  6,  2,  3,  7,  0,  1,  9, 99, 99, 99, 99, 99, 99, 99,
			  1,  6,  2,  1,  8,  6,  1,  9,  8,  8,  7,  6, 99, 99, 99, 99,
			 10,  7,  6, 10,  1,  7,  1,  3,  7, 99, 99, 99, 99, 99, 99, 99,
			 10,  7,  6,  1,  7, 10,  1,  8,  7,  1,  0,  8, 99, 99, 99, 99,
			  0,  3,  7,  0,  7, 10,  0, 10,  9,  6, 10,  7, 99, 99, 99, 99,
			  7,  6, 10,  7, 10,  8,  8, 10,  9, 99, 99, 99, 99, 99, 99, 99,
			  6,  8,  4, 11,  8,  6, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  3,  6, 11,  3,  0,  6,  0,  4,  6, 99, 99, 99, 99, 99, 99, 99,
			  8,  6, 11,  8,  4,  6,  9,  0,  1, 99, 99, 99, 99, 99, 99, 99,
			  9,  4,  6,  9,  6,  3,  9,  3,  1, 11,  3,  6, 99, 99, 99, 99,
			  6,  8,  4,  6, 11,  8,  2, 10,  1, 99, 99, 99, 99, 99, 99, 99,
			  1,  2, 10,  3,  0, 11,  0,  6, 11,  0,  4,  6, 99, 99, 99, 99,
			  4, 11,  8,  4,  6, 11,  0,  2,  9,  2, 10,  9, 99, 99, 99, 99,
			 10,  9,  3, 10,  3,  2,  9,  4,  3, 11,  3,  6,  4,  6,  3, 99,
			  8,  2,  3,  8,  4,  2,  4,  6,  2, 99, 99, 99, 99, 99, 99, 99,
			  0,  4,  2,  4,  6,  2, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  1,  9,  0,  2,  3,  4,  2,  4,  6,  4,  3,  8, 99, 99, 99, 99,
			  1,  9,  4,  1,  4,  2,  2,  4,  6, 99, 99, 99, 99, 99, 99, 99,
			  8,  1,  3,  8,  6,  1,  8,  4,  6,  6, 10,  1, 99, 99, 99, 99,
			 10,  1,  0, 10,  0,  6,  6,  0,  4, 99, 99, 99, 99, 99, 99, 99,
			  4,  6,  3,  4,  3,  8,  6, 10,  3,  0,  3,  9, 10,  9,  3, 99,
			 10,  9,  4,  6, 10,  4, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  4,  9,  5,  7,  6, 11, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  0,  8,  3,  4,  9,  5, 11,  7,  6, 99, 99, 99, 99, 99, 99, 99,
			  5,  0,  1,  5,  4,  0,  7,  6, 11, 99, 99, 99, 99, 99, 99, 99,
			 11,  7,  6,  8,  3,  4,  3,  5,  4,  3,  1,  5, 99, 99, 99, 99,
			  9,  5,  4, 10,  1,  2,  7,  6, 11, 99, 99, 99, 99, 99, 99, 99,
			  6, 11,  7,  1,  2, 10,  0,  8,  3,  4,  9,  5, 99, 99, 99, 99,
			  7,  6, 11,  5,  4, 10,  4,  2, 10,  4,  0,  2, 99, 99, 99, 99,
			  3,  4,  8,  3,  5,  4,  3,  2,  5, 10,  5,  2, 11,  7,  6, 99,
			  7,  2,  3,  7,  6,  2,  5,  4,  9, 99, 99, 99, 99, 99, 99, 99,
			  9,  5,  4,  0,  8,  6,  0,  6,  2,  6,  8,  7, 99, 99, 99, 99,
			  3,  6,  2,  3,  7,  6,  1,  5,  0,  5,  4,  0, 99, 99, 99, 99,
			  6,  2,  8,  6,  8,  7,  2,  1,  8,  4,  8,  5,  1,  5,  8, 99,
			  9,  5,  4, 10,  1,  6,  1,  7,  6,  1,  3,  7, 99, 99, 99, 99,
			  1,  6, 10,  1,  7,  6,  1,  0,  7,  8,  7,  0,  9,  5,  4, 99,
			  4,  0, 10,  4, 10,  5,  0,  3, 10,  6, 10,  7,  3,  7, 10, 99,
			  7,  6, 10,  7, 10,  8,  5,  4, 10,  4,  8, 10, 99, 99, 99, 99,
			  6,  9,  5,  6, 11,  9, 11,  8,  9, 99, 99, 99, 99, 99, 99, 99,
			  3,  6, 11,  0,  6,  3,  0,  5,  6,  0,  9,  5, 99, 99, 99, 99,
			  0, 11,  8,  0,  5, 11,  0,  1,  5,  5,  6, 11, 99, 99, 99, 99,
			  6, 11,  3,  6,  3,  5,  5,  3,  1, 99, 99, 99, 99, 99, 99, 99,
			  1,  2, 10,  9,  5, 11,  9, 11,  8, 11,  5,  6, 99, 99, 99, 99,
			  0, 11,  3,  0,  6, 11,  0,  9,  6,  5,  6,  9,  1,  2, 10, 99,
			 11,  8,  5, 11,  5,  6,  8,  0,  5, 10,  5,  2,  0,  2,  5, 99,
			  6, 11,  3,  6,  3,  5,  2, 10,  3, 10,  5,  3, 99, 99, 99, 99,
			  5,  8,  9,  5,  2,  8,  5,  6,  2,  3,  8,  2, 99, 99, 99, 99,
			  9,  5,  6,  9,  6,  0,  0,  6,  2, 99, 99, 99, 99, 99, 99, 99,
			  1,  5,  8,  1,  8,  0,  5,  6,  8,  3,  8,  2,  6,  2,  8, 99,
			  1,  5,  6,  2,  1,  6, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  1,  3,  6,  1,  6, 10,  3,  8,  6,  5,  6,  9,  8,  9,  6, 99,
			 10,  1,  0, 10,  0,  6,  9,  5,  0,  5,  6,  0, 99, 99, 99, 99,
			  0,  3,  8,  5,  6, 10, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			 10,  5,  6, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			 11,  5, 10,  7,  5, 11, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			 11,  5, 10, 11,  7,  5,  8,  3,  0, 99, 99, 99, 99, 99, 99, 99,
			  5, 11,  7,  5, 10, 11,  1,  9,  0, 99, 99, 99, 99, 99, 99, 99,
			 10,  7,  5, 10, 11,  7,  9,  8,  1,  8,  3,  1, 99, 99, 99, 99,
			 11,  1,  2, 11,  7,  1,  7,  5,  1, 99, 99, 99, 99, 99, 99, 99,
			  0,  8,  3,  1,  2,  7,  1,  7,  5,  7,  2, 11, 99, 99, 99, 99,
			  9,  7,  5,  9,  2,  7,  9,  0,  2,  2, 11,  7, 99, 99, 99, 99,
			  7,  5,  2,  7,  2, 11,  5,  9,  2,  3,  2,  8,  9,  8,  2, 99,
			  2,  5, 10,  2,  3,  5,  3,  7,  5, 99, 99, 99, 99, 99, 99, 99,
			  8,  2,  0,  8,  5,  2,  8,  7,  5, 10,  2,  5, 99, 99, 99, 99,
			  9,  0,  1,  5, 10,  3,  5,  3,  7,  3, 10,  2, 99, 99, 99, 99,
			  9,  8,  2,  9,  2,  1,  8,  7,  2, 10,  2,  5,  7,  5,  2, 99,
			  1,  3,  5,  3,  7,  5, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  0,  8,  7,  0,  7,  1,  1,  7,  5, 99, 99, 99, 99, 99, 99, 99,
			  9,  0,  3,  9,  3,  5,  5,  3,  7, 99, 99, 99, 99, 99, 99, 99,
			  9,  8,  7,  5,  9,  7, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  5,  8,  4,  5, 10,  8, 10, 11,  8, 99, 99, 99, 99, 99, 99, 99,
			  5,  0,  4,  5, 11,  0,  5, 10, 11, 11,  3,  0, 99, 99, 99, 99,
			  0,  1,  9,  8,  4, 10,  8, 10, 11, 10,  4,  5, 99, 99, 99, 99,
			 10, 11,  4, 10,  4,  5, 11,  3,  4,  9,  4,  1,  3,  1,  4, 99,
			  2,  5,  1,  2,  8,  5,  2, 11,  8,  4,  5,  8, 99, 99, 99, 99,
			  0,  4, 11,  0, 11,  3,  4,  5, 11,  2, 11,  1,  5,  1, 11, 99,
			  0,  2,  5,  0,  5,  9,  2, 11,  5,  4,  5,  8, 11,  8,  5, 99,
			  9,  4,  5,  2, 11,  3, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  2,  5, 10,  3,  5,  2,  3,  4,  5,  3,  8,  4, 99, 99, 99, 99,
			  5, 10,  2,  5,  2,  4,  4,  2,  0, 99, 99, 99, 99, 99, 99, 99,
			  3, 10,  2,  3,  5, 10,  3,  8,  5,  4,  5,  8,  0,  1,  9, 99,
			  5, 10,  2,  5,  2,  4,  1,  9,  2,  9,  4,  2, 99, 99, 99, 99,
			  8,  4,  5,  8,  5,  3,  3,  5,  1, 99, 99, 99, 99, 99, 99, 99,
			  0,  4,  5,  1,  0,  5, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  8,  4,  5,  8,  5,  3,  9,  0,  5,  0,  3,  5, 99, 99, 99, 99,
			  9,  4,  5, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  4, 11,  7,  4,  9, 11,  9, 10, 11, 99, 99, 99, 99, 99, 99, 99,
			  0,  8,  3,  4,  9,  7,  9, 11,  7,  9, 10, 11, 99, 99, 99, 99,
			  1, 10, 11,  1, 11,  4,  1,  4,  0,  7,  4, 11, 99, 99, 99, 99,
			  3,  1,  4,  3,  4,  8,  1, 10,  4,  7,  4, 11, 10, 11,  4, 99,
			  4, 11,  7,  9, 11,  4,  9,  2, 11,  9,  1,  2, 99, 99, 99, 99,
			  9,  7,  4,  9, 11,  7,  9,  1, 11,  2, 11,  1,  0,  8,  3, 99,
			 11,  7,  4, 11,  4,  2,  2,  4,  0, 99, 99, 99, 99, 99, 99, 99,
			 11,  7,  4, 11,  4,  2,  8,  3,  4,  3,  2,  4, 99, 99, 99, 99,
			  2,  9, 10,  2,  7,  9,  2,  3,  7,  7,  4,  9, 99, 99, 99, 99,
			  9, 10,  7,  9,  7,  4, 10,  2,  7,  8,  7,  0,  2,  0,  7, 99,
			  3,  7, 10,  3, 10,  2,  7,  4, 10,  1, 10,  0,  4,  0, 10, 99,
			  1, 10,  2,  8,  7,  4, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  4,  9,  1,  4,  1,  7,  7,  1,  3, 99, 99, 99, 99, 99, 99, 99,
			  4,  9,  1,  4,  1,  7,  0,  8,  1,  8,  7,  1, 99, 99, 99, 99,
			  4,  0,  3,  7,  4,  3, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  4,  8,  7, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  9, 10,  8, 10, 11,  8, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  3,  0,  9,  3,  9, 11, 11,  9, 10, 99, 99, 99, 99, 99, 99, 99,
			  0,  1, 10,  0, 10,  8,  8, 10, 11, 99, 99, 99, 99, 99, 99, 99,
			  3,  1, 10, 11,  3, 10, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  1,  2, 11,  1, 11,  9,  9, 11,  8, 99, 99, 99, 99, 99, 99, 99,
			  3,  0,  9,  3,  9, 11,  1,  2,  9,  2, 11,  9, 99, 99, 99, 99,
			  0,  2, 11,  8,  0, 11, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  3,  2, 11, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  2,  3,  8,  2,  8, 10, 10,  8,  9, 99, 99, 99, 99, 99, 99, 99,
			  9, 10,  2,  0,  9,  2, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  2,  3,  8,  2,  8, 10,  0,  1,  8,  1, 10,  8, 99, 99, 99, 99,
			  1, 10,  2, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  1,  3,  8,  9,  1,  8, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  0,  9,  1, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			  0,  3,  8, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99
			 
        };
        
        private static byte[] CornerIndexA = { 0, 1, 2, 3, 4, 5, 6, 7, 0, 1, 2, 3 };

        private static byte[] CornerIndexB = { 1, 2, 3, 0, 5, 6, 7, 4, 4, 5, 6, 7 };
        
        #endregion
    }

    public struct GridCell
    {
        public Vector3[] P;
        public float[] Density;
        public BlockType[] Type;
    }

    public struct Triangle
    {
        public Vector3[] Vertices;
    }
}