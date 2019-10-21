/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:22 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Numerics;
using System.Collections.Generic;
using Hedra.Numerics;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    /// <summary>
    /// Description of Vertex.
    /// </summary>
    public class Vertex
    {
        private const int NoIndex = -1;
    
        public Vector3 Position {get; private set;}
        public int ColorIndex {get; set;}
        public int NormalIndex {get; set;}
        public Vertex DuplicateVertex {get; set;}
        public int Index {get; private set;}
        public float Length {get; private set;}
        private List<Vector3> Tangents = new List<Vector3>();
        public Vector3 AveragedTangent {get; private set;}
        
        public VertexSkinData WeightsData {get; private set;}
        
        public Vertex(int index, Vector3 Position, VertexSkinData weightsData){
            this.ColorIndex = NoIndex;
            this.NormalIndex = NoIndex;
            
            this.Index = index;
            this.WeightsData = weightsData;
            this.Position = Position;
            this.Length = Position.Length();
        }
        
        public void AddTangent(Vector3 tangent){
            Tangents.Add(tangent);
        }
        
        public void AverageTangents()
        {
            if(Tangents.Count == 0){
                return;
            }
            for(int i = 0; i < Tangents.Count; i++) {
                AveragedTangent += Tangents[i];
            }
            AveragedTangent = AveragedTangent.Normalized();
        }

        public bool IsSet{
            get{ return ColorIndex != NoIndex && NormalIndex != NoIndex; }
        }
        
        public bool HasSameTextureAndNormal(int textureIndexOther,int normalIndexOther){
            return textureIndexOther == ColorIndex && normalIndexOther == NormalIndex;
        }
    }
}
