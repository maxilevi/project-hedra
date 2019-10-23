/*
 * Author: Zaphyk
 * Date: 01/02/2016
 * Time: 09:41 p.m.
 *
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Hedra.Core;
using System.Numerics;

namespace Hedra.Engine.Rendering
{
    /// <summary>
    /// An object which represents the rendering information of a cube
    /// </summary>
    public class CubeData : DataContainer
    {    
        public static uint[][] IndexArray = new uint[][]{
            new uint[]{4, 5, 6,  6, 7, 4},
            new uint[]{8, 9,10,  10,11, 8},      // top
            new uint[]{12,13,14, 14,15,12},      // left            
            new uint[]{20,21,22, 22,23,20},      //back
            new uint[]{16,17,18, 18,19,16},      // bottom    
            new uint[]{0, 1, 2,  2, 3, 0},    // front
            new uint[]{
                4, 5, 6,  6, 7, 4,
                8, 9,10,  10,11, 8,      // top
                12,13,14, 14,15,12,      // left            
                20,21,22, 22,23,20,      //back
                16,17,18, 18,19,16,
                0, 1, 2,  2, 3, 0
            }
        };
        
        private void Initialize(){
            base.Normals = new Vector3[]{
                new Vector3(0,0,1), new Vector3(0,0,1), new Vector3(0,0,1), new Vector3(0,0,1),
                new Vector3(1,0,0), new Vector3(1,0,0), new Vector3(1,0,0), new Vector3(1,0,0),
                new Vector3(0,1,0), new Vector3(0,1,0), new Vector3(0,1,0), new Vector3(0,1,0),
                new Vector3(-1,0,0), new Vector3(-1,0,0), new Vector3(-1,0,0), new Vector3(-1,0,0),
                new Vector3(0,-1,0), new Vector3(0,-1,0), new Vector3(0,-1,0), new Vector3(0,-1,0),
                new Vector3(0,0,-1), new Vector3(0,0,-1), new Vector3(0,0,-1), new Vector3(0,0,-1)
            };
            
            VerticesArrays = new Vector3[] { 
                new Vector3(1, 1, 1),   new Vector3(0, 1, 1), new Vector3(0,0, 1),  new Vector3( 1,0, 1),   // v0,v1,v2,v3 (front)
                new Vector3(1, 1, 1),   new Vector3(1,0, 1),  new Vector3( 1,0,0),  new Vector3( 1, 1,0),   // v0,v3,v4,v5 (right)
                new Vector3(1, 1, 1),   new Vector3(1, 1,0),  new Vector3(0, 1,0),  new Vector3(0, 1, 1),   // v0,v5,v6,v1 (top)
                new Vector3(0, 1, 1),  new Vector3(0, 1,0), new Vector3( 0,0,0), new Vector3( 0,0, 1),   // v1,v6,v7,v2 (left)
                new Vector3(0,0,0),  new Vector3(1,0,0),  new Vector3( 1,0, 1),  new Vector3(0,0, 1),   // v7,v4,v3,v2 (bottom)
                new Vector3(1,0,0),   new Vector3(0,0,0), new Vector3(0, 1,0),  new Vector3( 1, 1,0)
            };
            this.HasNormals = true;
        }
        public CubeData()
        {
            this.Initialize();
        }
        
        public CubeData(Vector4[] Color, Vector3 Position, List<uint> Ind, List<byte> Faces, Vector3[] Verts){
            this.Initialize();
            this.Color = Color?.Clone() as Vector4[];
            this.Position = Position;
            this.Indices = (Ind?.ToArray().Clone() as uint[])?.ToList();
            this.FacesIndex = (Faces?.ToArray().Clone() as byte[])?.ToList();
            this.VerticesArrays = Verts.Clone() as Vector3[];
        }
        
        public CubeData(Vector4[] Color, Vector3 Position){
            this.Position = Position;
            this.Color = Color;
            this.Initialize();
            this.TransformVerts(Position);
        }
        public CubeData(Vector4[] Color, Vector3 Position, float Scale){
            this.Position = Position;
            this.Color = Color;
            this.Initialize();
            this.Scale(Scale);
            this.TransformVerts(Position);
        }
        
        public static CubeData CutCubeFace(CubeData Data, Face Type){
            Data.AddFace(Type);
            return Data;
        }
        
        public void AddFace(Face Type)
        {
            Indices.AddRange(IndexArray[(int)Type]);
            FacesIndex.Add( (byte)Type);
        }
        
        public CubeData Clone(){
            return new CubeData(this.Color, this.Position, this.Indices, this.FacesIndex, this.VerticesArrays);
        }
        
        public Vector3[] ToVertexArray(){
            var newVerts = new List<Vector3>();
            for(var i = 0; i < Indices.Count; i++){
                newVerts.Add(VerticesArrays[Indices[i]]);
            }
            return newVerts.ToArray();
        }

        public static Vector4[] CreateCubeColor(Vector4 Color){
            return new Vector4[] { 
                Color, Color, Color, Color,
                Color, Color, Color, Color,
                Color, Color, Color, Color,
                Color, Color, Color, Color,
                Color, Color, Color, Color,
                Color, Color, Color, Color
            };
        }
    }

    public enum Face{
        RIGHT,
        UP,
        LEFT,
        BACK,
        DOWN,
        FRONT,
        ALL
    }
}