/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 02:01 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 
using System;
using System.Collections.Generic;
using OpenTK;
using Hedra.Engine.Generation;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of SurfaceNets.
	/// </summary>
	public static class SurfaceNets
	{
		private static int[] Buffer = new int[4096];
		private static int[] CubeEdges = new int[24], EdgeTable = new int[256];
		
		static SurfaceNets(){
			//Precompute
			var k = 0;
			for (var i = 0; i < 8; i++)
	        {
	            for (var j = 1; j <= 4; j <<= 1)
	            {
	                var p = i ^ j;
	                if (i <= p)
	                {
	                    CubeEdges[k++] = i;
	                    CubeEdges[k++] = p;
	                }
	            }
	        }
	 
	        //Initialize Intersection Table
	        for (var i = 0; i < 256; i++)
	        {
	            int em = 0;
	            for (var j = 0; j < 24; j += 2)
	            {
	                var a = Convert.ToBoolean(i & (1 << CubeEdges[j]));
	                var b = Convert.ToBoolean(i & (1 << CubeEdges[j + 1]));
	                em |= a != b ? (1 << (j >> 1)) : 0;
	            }
	            EdgeTable[i] = em;
	        }
		}
		
		public static VertexData Compute(Block[][][] voxels)
	    {
	        //Load
	        Buffer = new int[4096];
	        VertexData Data = new VertexData();
	        int width = voxels.Length;
	        int height = voxels[0].Length;
	        int depth = voxels[0][0].Length;
	        var vertices = new List<Vector3>();
	        var faces = new List<int>();
	        var n = 0;
	        var pos = new int[3];
	        var R = new int[]{1, width + 1, (width + 1) * (height + 1)};
	        var grid = new float[8];
	        var BufferNumber = 1;
	 
	        //Resize Buffer
	        if (R[2] * 2 > Buffer.Length)
	            Buffer = new int[R[2] * 2];
	 
	        #region March
	        //March Over Voxels
	        for (pos[2] = 0; pos[2] < depth - 1; pos[2]++, BufferNumber ^= 1, R[2] = -R[2] )
	        {
	            //Buffer Pointer
	            var BufferIndex = 1 + (width + 1) * (1 + BufferNumber * (height + 1));
	 
	            for(pos[1] = 0; pos[1] < height - 1; pos[1]++, n++, BufferIndex += 2)
	            {
	                for (pos[0] = 0; pos[0] < width - 1; pos[0]++, n++, BufferIndex++)
	                {
	                    //8 field values around vertex, plus 8-bit mask
	                    var mask = 0;
	                    var g = 0;
	                    var index = n;
	                    for (var k = 0; k < 2; k++, index += width * (height - 2))
	                    {
	                        for (var j = 0; j < 2; j++, index += width - 2)
	                        {
	                            for (var i = 0; i < 2; i++, g++, index++)
	                            {
	                            	var p = voxels[index / (width * width)][(index / depth) % height][index % depth];
	                                grid[g] = p;
	                                mask |= (p < 0) ? (1 << g) : 0;
	                            }
	                        }
	                    }
	 
	                    //Early Termination Check
	                    if (mask == 0 || mask == 0xff)
	                    {
	                        continue;
	                    }
	 
	                    //Sum Edge Intersections
	                    var edgeMask = EdgeTable[mask];
	                    var vertex = new Vector3();
	                    var edgeIndex = 0;
	 
	                    //For Every Cube Edge
	                    for (var i = 0; i < 12; i++)
	                    {
	                        //Use Edge Mask to Check if Crossed
	                        if (!Convert.ToBoolean(edgeMask & (1 << i)))
	                        {
	                            continue;
	                        }
	 
	                        //If So, Increment Edge Crossing #
	                        edgeIndex++;
	 
	                        //Find Intersection Point
	                        var e0 = CubeEdges[i << 1];
	                        var e1 = CubeEdges[(i << 1) + 1];
	                        var g0 = grid[e0];
	                        var g1 = grid[e1];
	                        var t = g0 - g1;
	                        if (Math.Abs(t) > 1e-16)
	                            t = g0 / t;
	                        else
	                            continue;
	 
	                        //Interpolate Vertices, Add Intersections
	                        for(int j = 0, k = 1; j < 3; j++, k <<=1)
	                        {
	                            var a = e0 & k;
	                            var b = e1 & k;
	                            if (a != b)
	                                vertex[j] += Convert.ToBoolean(a) ? 1f - t : t;
	                            else
	                                vertex[j] += Convert.ToBoolean(a) ? 1f : 0;
	                        }
	                    }
	 
	                    //Average Edge Intersections, Add to Coordinate
	                    var s = 1f / edgeIndex;
	                    for(var i = 0; i < 3; i++)
	                    {
	                        vertex[i] = pos[i] + s * vertex[i];
	                    }
	 
	                    //Add Vertex to Buffer, Store Pointer to Vertex Index
	                    Buffer[BufferIndex] = vertices.Count;
	                    Control.msg = Buffer[BufferIndex];
	                    vertices.Add(vertex);
	 
	                    //Add Faces (Loop Over 3 Base Components)
	                    for (var i = 0; i < 3; i ++)
	                    {
	                        //First 3 Entries Indicate Crossings on Edge
	                        if(!Convert.ToBoolean(edgeMask & (1 << i)))
	                        {
	                            continue;
	                        }
	 
	                        //i - Axes, iu, iv - Ortho Axes
	                        var iu = (i + 1) % 3;
	                        var iv = (i + 2) % 3;
	 
	                        //Skip if on Boundary
	                        if (pos[iu] == 0 || pos[iv] == 0)
	                            continue;
	 
	                        //Otherwise, Look Up Adjacent Edges in Buffer
	                        var du = R[iu];
	                        var dv = R[iv];
	 
	                        //Flip Orientation Depending on Corner Sign
	                        if (Convert.ToBoolean(mask & 1))
	                        {
	                            faces.Add(Buffer[BufferIndex]);
	                            faces.Add(Buffer[BufferIndex - du]);
	                            faces.Add(Buffer[BufferIndex - du - dv]);
	                            faces.Add(Buffer[BufferIndex - dv]);
	                        }
	                        else
	                        {
	                            faces.Add(Buffer[BufferIndex]);
	                            faces.Add(Buffer[BufferIndex - dv]);
	                            faces.Add(Buffer[BufferIndex - du - dv]);
	                            faces.Add(Buffer[BufferIndex - du]);
	                        }
	                    }
	                   
	                }
	            }
	        }
	 		#endregion
	 		
	 		Data.Indices.AddRange(faces.ToArray() as uint[]);
	        return Data;
	    }
	}
}*/
