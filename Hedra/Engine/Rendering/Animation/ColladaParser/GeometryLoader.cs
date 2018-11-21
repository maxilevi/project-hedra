/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:20 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Globalization;
using OpenTK;
using System.Xml;
using System.Collections.Generic;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    /// <summary>
    /// Description of GeometryLoader.
    /// </summary>
    public class GeometryLoader
    {
        private readonly XmlNode MeshData;
    
        private readonly List<VertexSkinData> VertexWeights;
        
        private int[] JointIdsArray;
        private float[] WeightsArray;
    
        List<Vertex> Vertices = new List<Vertex>();
        List<Vector3> Colors = new List<Vector3>();
        List<Vector3> Normals = new List<Vector3>();
        List<uint> Indices = new List<uint>();
        
        private Matrix4 Correction = Matrix4.CreateRotationX(-90f * Mathf.Radian);
        
        public GeometryLoader(XmlNode GeometryNode, List<VertexSkinData> VertexWeights) {
            this.VertexWeights = VertexWeights;
            this.MeshData = GeometryNode["geometry"]["mesh"];
        }
        
        public ModelData ExtractModelData()
        {
            var polyNode = this.MeshData["polylist"] ?? this.MeshData["triangles"];
            this.ReadRawData(polyNode);
            this.AssembleVertices(polyNode);
            this.RemoveUnusedVertices();
            this.InitArrays();
            this.ConvertDataToArrays();
            
            List<Vector3> Positions = new List<Vector3>(), JointIds = new List<Vector3>(),
            Weights = new List<Vector3>(), NormalsList = new List<Vector3>(), ColorsList = new List<Vector3>();
            for(int i = 0; i < Vertices.Count; i++){
                Positions.Add(Vertices[i].Position);
                NormalsList.Add(Normals[Vertices[i].NormalIndex]);
                ColorsList.Add(Colors[Vertices[i].ColorIndex]);
            }
            for(int i = 0; i < JointIdsArray.Length; i += 3){
                JointIds.Add( new Vector3(JointIdsArray[i+0], JointIdsArray[i+1], JointIdsArray[i+2]) );
            }
            
            for(int i = 0; i < WeightsArray.Length; i += 3){
                float s = WeightsArray[i+0], q = WeightsArray[i+1], r = WeightsArray[i+2];
                
                if(1-(s+q) != 0 && r == 0) q = 1-s;
                if(1-(s+q+r) != 0 && r != 0) r = 1-(s+q);
                
                Weights.Add(new Vector3(s,q,r));
            }
            return new ModelData(Positions.ToArray(), ColorsList.ToArray(), NormalsList.ToArray(), Indices.ToArray(), JointIds.ToArray(), Weights.ToArray());
        }
    
        private void ReadRawData(XmlNode PolyNode) {
            this.ReadPositions();
            this.ReadNormals(PolyNode);
            this.ReadColors(PolyNode);
        }
    
        private void ReadPositions() {
            string positionsId = MeshData["vertices"]["input"].GetAttribute("source").Substring(1);
            XmlNode positionsData = MeshData.ChildWithAttribute("source", "id", positionsId)["float_array"];
            int count = int.Parse( positionsData.GetAttribute("count").Value );
            string[] posData = positionsData.InnerText.Split(' ');
            for (int i = 0; i < count/3; i++) {
                float x = float.Parse( posData[i * 3], NumberStyles.Any, CultureInfo.InvariantCulture);
                float y = float.Parse( posData[i * 3 + 1], NumberStyles.Any, CultureInfo.InvariantCulture);
                float z = float.Parse( posData[i * 3 + 2], NumberStyles.Any, CultureInfo.InvariantCulture);
                Vector4 position = new Vector4(x, y, z, 1);
                position = Vector4.Transform(position, Correction);
                Vertices.Add(new Vertex(Vertices.Count, position.Xyz, VertexWeights[Vertices.Count] ));
            }
        }
    
        private void ReadNormals(XmlNode PolyNode) {
            string normalsId = PolyNode.ChildWithAttribute("input", "semantic", "NORMAL").GetAttribute("source").Value.Substring(1);
            XmlNode normalsData = MeshData.ChildWithAttribute("source", "id", normalsId)["float_array"];
            int count = int.Parse(normalsData.GetAttribute("count").Value);
            string[] normData = normalsData.InnerText.Split(' ');
            for (int i = 0; i < count/3; i++) {
                float x = float.Parse(normData[i * 3], NumberStyles.Any, CultureInfo.InvariantCulture);
                float y = float.Parse(normData[i * 3 + 1], NumberStyles.Any, CultureInfo.InvariantCulture);
                float z = float.Parse(normData[i * 3 + 2], NumberStyles.Any, CultureInfo.InvariantCulture);
                Vector4 norm = new Vector4(x, y, z, 1);
                norm = Vector4.Transform(norm, Correction);
                Normals.Add( norm.Xyz );
            }
        }
    
        private void ReadColors(XmlNode PolyNode) {
            string ColorsId = PolyNode.ChildWithAttribute("input", "semantic", "COLOR")
                    .GetAttribute("source").Value.Substring(1);
            XmlNode ColorsNode = MeshData.ChildWithAttribute("source", "id", ColorsId)["float_array"];
            int count = int.Parse(ColorsNode.GetAttribute("count").Value);
            string[] ColorsData = ColorsNode.InnerText.Split(' ');
            for (int i = 0; i < count/3; i++) {
                float s = float.Parse(ColorsData[i * 3 + 0], NumberStyles.Any, CultureInfo.InvariantCulture);
                float t = float.Parse(ColorsData[i * 3 + 1], NumberStyles.Any, CultureInfo.InvariantCulture);
                float q = float.Parse(ColorsData[i * 3 + 2], NumberStyles.Any, CultureInfo.InvariantCulture);
                Colors.Add(new Vector3(s, t , q));
            }
        }
        
        private void AssembleVertices(XmlNode PolyNode)
        {
            int typeCount = PolyNode.ChildrenCount("input");
            string[] indexData = PolyNode["p"].InnerText.Split(' ');
            for(int i=0; i < indexData.Length / typeCount; i++){
                int PositionIndex = int.Parse(indexData[i * typeCount]);
                int NormalIndex = int.Parse(indexData[i * typeCount + 1]);
                int TexCoordIndex = int.Parse(indexData[i * typeCount + 2]);
                
                this.ProcessVertex(PositionIndex, NormalIndex, TexCoordIndex);
            }
        }
        
    
        private Vertex ProcessVertex(int posIndex, int normIndex, int texIndex) {
            Vertex CurrentVertex = Vertices[posIndex];
            
            if (!CurrentVertex.IsSet) {
                
                CurrentVertex.ColorIndex = texIndex;
                CurrentVertex.NormalIndex = normIndex;
                Indices.Add( (uint) posIndex);
                return CurrentVertex;
                
            } else {
                
                return DealWithAlreadyProcessedVertex(CurrentVertex, texIndex, normIndex);
            }
        }
    
        private float ConvertDataToArrays() {
            float furthestPoint = 0;
            for (int i = 0; i < Vertices.Count; i++) {
                Vertex currentVertex = Vertices[i];
                if (currentVertex.Length > furthestPoint) {
                    furthestPoint = currentVertex.Length;
                }
                VertexSkinData weights = currentVertex.WeightsData;
                JointIdsArray[i * 3] = weights.JointIds[0];
                JointIdsArray[i * 3 + 1] = weights.JointIds[1];
                JointIdsArray[i * 3 + 2] = weights.JointIds[2];
                WeightsArray[i * 3] = weights.Weights[0];
                WeightsArray[i * 3 + 1] = weights.Weights[1];
                WeightsArray[i * 3 + 2] = weights.Weights[2];
    
            }
            return furthestPoint;
        }
    
        private Vertex DealWithAlreadyProcessedVertex(Vertex previousVertex, int newTextureIndex, int newNormalIndex) {
            if (previousVertex.HasSameTextureAndNormal(newTextureIndex, newNormalIndex)) {
                Indices.Add( (uint) previousVertex.Index);
                return previousVertex;
            } else {
                Vertex anotherVertex = previousVertex.DuplicateVertex;
                if (anotherVertex != null) {
                    return DealWithAlreadyProcessedVertex(anotherVertex, newTextureIndex, newNormalIndex);
                } else {
                    Vertex duplicateVertex = new Vertex(Vertices.Count, previousVertex.Position, previousVertex.WeightsData);
                    duplicateVertex.ColorIndex = newTextureIndex;
                    duplicateVertex.NormalIndex = newNormalIndex;
                    previousVertex.DuplicateVertex = duplicateVertex;
                    Vertices.Add(duplicateVertex);
                    Indices.Add( (uint) duplicateVertex.Index);
                    return duplicateVertex;
                }
    
            }
        }
        
        private void InitArrays(){
            this.JointIdsArray = new int[Vertices.Count * 3];
            this.WeightsArray = new float[Vertices.Count * 3];
        }
    
        private void RemoveUnusedVertices() {
            for (int i = 0; i < Vertices.Count; i++){
                Vertices[i].AverageTangents();
                if (!Vertices[i].IsSet) {
                    Vertices[i].ColorIndex = 0;
                    Vertices[i].NormalIndex = 0;
                }
            }
        }
    }
}
