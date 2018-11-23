/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/05/2016
 * Time: 10:04 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.Rendering.Geometry
{
    /// <summary>
    /// Description of MeshSimplifier.
    /// </summary>
    public static class MeshSimplifier
    {
        class Triangle{
            public int[] v;
            public float[] err;
            public int deleted, dirty;
            public Vector3 n;
        };
        
        struct Vertex{
            public Vector3 p;
            public int tstart, tcount;
            public SymetricMatrix q;
            public int border;
        };
        
        struct Ref{
            public int tid, tvertex;
        };
        
        private static List<Triangle> triangles = new List<Triangle>();
        private static List<Vertex> vertices = new List<Vertex>();
        private static List<Ref> refs = new List<Ref>();
        
        public static VertexData Simplify( VertexData Data, int LOD, int target_count = 100, float agresiveness = 7)
        {
            return Data;
            if(LOD == 1) return Data;
            
            for(int i = 0; i < triangles.Count;i++)
                triangles[i].deleted = 0;
            
            int deleted_triangles = 0;
            List<int> deleted0 = new List<int>(), deleted1 = new List<int>();
            
            for(int i = 0; i < 100; i++){
                if(triangles.Count - deleted_triangles <= target_count)
                    break;
                
                //if( i % 5 == 0)
                //    update_mesh(i);
                
                for(int j = 0; j < triangles.Count; j++)
                    triangles[j].dirty = 0;
                
                float threshold = 0.000000001f * (float) Math.Pow(i+3, agresiveness);
                
                for(int j = 0; j < triangles.Count; j++){
                }
            }
            
            return Data;
        }
    }
}
