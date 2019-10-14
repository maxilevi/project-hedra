/*
 * Author: Zaphyk
 * Date: 20/03/2016
 * Time: 03:11 p.m.
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Rendering;
using System.Numerics;

namespace Hedra.Engine.Rendering
{
    /// <summary>
    /// Description of IDataContainer.
    /// </summary>
    public abstract class DataContainer
    {
        public Vector3[] VerticesArrays;
        public List<uint> Indices = new List<uint>();
        public List<byte> FacesIndex = new List<byte>( (int) Face.ALL);
        public Vector3[] Normals;
        public Vector4[] Color;
        public Vector3 Position {get; set;}
        public bool HasNormals{get; set;}
        
        public void TransformVerts(Vector3 v){
            for(int i=0; i<VerticesArrays.Length;i++){ 
                VerticesArrays[i] += v;
            }
        }
        
        public void TransformVerts(Matrix4x4 m){
            for(int i=0; i<VerticesArrays.Length;i++){
                VerticesArrays[i] = Vector3.Transform(VerticesArrays[i], m);
            }
        }
        
        public void Scale(float Scalar){
            for(int i=0; i<VerticesArrays.Length;i++){
                VerticesArrays[i] *= Scalar; 
            }
        }
        
        public void Scale(Vector3 Scalar){
            for(int i=0; i<VerticesArrays.Length;i++){
                VerticesArrays[i] *= Scalar;
            }
        }
        
        public VertexData ToVertexData(){
            var data = new VertexData
            {
                Normals = Normals.ToArray().ToList(),
                Vertices = VerticesArrays.ToArray().ToList(),
                Indices = Indices,
                Colors = Color.ToArray().ToList()
            };
            return data;
        }
    }
}
