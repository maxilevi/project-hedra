/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 16/04/2017
 * Time: 04:33 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using OpenTK;

namespace Hedra.Engine.Rendering.Geometry
{
    /// <summary>
    /// Description of VertexOcclusion.
    /// </summary>
    public static class VertexOcclusion
    {
        public static VertexData Bake(Vector3 Position, VertexData Data){
               /*int Samples = 100;
            List<Vector3> Normals = Data.Normals;
            List<Vector3> Vertices = Data.Vertices;
                
            Vector3[] Rotations = new Vector3[Samples];
            
            var Radius = Math.Max( Math.Max(Data.SupportPoint(-Vector3.UnitX).Z + Data.SupportPoint(Vector3.UnitX).X,
                                                        Data.SupportPoint(-Vector3.UnitY).Y + Data.SupportPoint(Vector3.UnitY).Y),
                                                     Data.SupportPoint(-Vector3.UnitZ).Z + Data.SupportPoint(Vector3.UnitZ).Z);
            for (var i = 0; i < Samples; i++) {
                Rotations[i] = Position + (Utils.Rng.NextUnitSphere() * Radius);
            }
            
            
            for(int i = 0; i < Data.Vertices.Count; i++){
                Vector4 Occlusion = Vector4.One;
                float Hits = 0f;
                
                Vector3[] Neighbours = NeighbourVertices(Data, Data.Vertices[i], i);
                for(int k = 0; k < Neighbours.Length; k++){
                    if(Neighbours[k].Y > Data.Vertices[i].Y+.1f)
                        Hits += (1f/ Neighbours.Length);
                }
                Occlusion *= Mathf.Clamp(Hits - .5f, 0f, 1f) * .5f;
                Data.Colors[i] *= (1f-Occlusion.X);
            }*/
  
            return Data;            
        }
        
        private static Vector3[] NeighbourVertices(VertexData Data, Vector3 Vertex, int Index){
            List<Vector3> Neighbours = new List<Vector3>();
            
            for(int i = (int) Math.Max(Index-8,0); i < (int) Math.Min(Index+8, Data.Vertices.Count); i++){
                if(Data.Vertices[i] != Vertex && (Data.Vertices[i] - Vertex).LengthSquared < 2*2 )
                    Neighbours.Add(Data.Vertices[i]);
            }
            return Neighbours.ToArray();
        }
    }
}
