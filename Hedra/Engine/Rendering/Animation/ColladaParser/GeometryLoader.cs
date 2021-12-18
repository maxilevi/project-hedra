/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:20 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Xml;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    /// <summary>
    ///     Description of GeometryLoader.
    /// </summary>
    public class GeometryLoader
    {
        private readonly XmlNode MeshData;

        private readonly SkinningData Skinning;
        private readonly List<Vector3> Colors = new List<Vector3>();

        private readonly Matrix4x4 Correction = Matrix4x4.CreateRotationX(-90f * Mathf.Radian);
        private readonly List<uint> Indices = new List<uint>();

        private int[] JointIdsArray;
        private readonly List<Vector3> Normals = new List<Vector3>();

        private readonly List<Vertex> Vertices = new List<Vertex>();
        private float[] WeightsArray;

        public GeometryLoader(XmlNode GeometryNode, SkinningData Skinning)
        {
            this.Skinning = Skinning;
            MeshData = GeometryNode["geometry"]["mesh"];
        }

        public ModelData ExtractModelData(bool FlipNormals)
        {
            var polyNode = MeshData["polylist"] ?? MeshData["triangles"];
            ReadRawData(polyNode);
            AssembleVertices(polyNode);
            RemoveUnusedVertices();
            InitArrays();
            ConvertDataToArrays();

            var positions = new List<Vector3>();
            var jointIds = new List<Vector3>();
            var weights = new List<Vector3>();
            var normalsList = new List<Vector3>();
            var colorsList = new List<Vector3>();
            for (var i = 0; i < Vertices.Count; i++)
            {
                positions.Add(Vertices[i].Position);
                normalsList.Add((FlipNormals ? -1 : 1) * Normals[Vertices[i].NormalIndex]);
                colorsList.Add(Colors[Vertices[i].ColorIndex]);
            }

            for (var i = 0; i < JointIdsArray.Length; i += 3)
                jointIds.Add(new Vector3(JointIdsArray[i + 0], JointIdsArray[i + 1], JointIdsArray[i + 2]));

            for (var i = 0; i < WeightsArray.Length; i += 3)
            {
                float s = WeightsArray[i + 0], q = WeightsArray[i + 1], r = WeightsArray[i + 2];

                if (Math.Abs(1 - (s + q)) > 0.05f && Math.Abs(r) < 0.05f) q = 1 - s;
                if (Math.Abs(1 - (s + q + r)) > 0.05f && Math.Abs(r) > 0.05f) r = 1 - (s + q);

                weights.Add(new Vector3(s, q, r));
            }

            VertexData.TrimExcess(positions);
            VertexData.TrimExcess(jointIds);
            VertexData.TrimExcess(weights);
            VertexData.TrimExcess(normalsList);
            VertexData.TrimExcess(colorsList);
            return new ModelData(positions.ToArray(), colorsList.ToArray(), normalsList.ToArray(), Indices.ToArray(),
                jointIds.ToArray(), weights.ToArray(), Skinning.JointOrder.ToArray());
        }

        private void ReadRawData(XmlNode PolyNode)
        {
            ReadPositions();
            ReadNormals(PolyNode);
            ReadColors(PolyNode);
        }

        private void ReadPositions()
        {
            var positionsId = MeshData["vertices"]["input"].GetAttribute("source").Substring(1);
            XmlNode positionsData = MeshData.ChildWithAttribute("source", "id", positionsId)["float_array"];
            var count = int.Parse(positionsData.GetAttribute("count").Value);
            var posData = positionsData.InnerText.Split(' ');
            for (var i = 0; i < count / 3; i++)
            {
                var x = float.Parse(posData[i * 3], NumberStyles.Any, CultureInfo.InvariantCulture);
                var y = float.Parse(posData[i * 3 + 1], NumberStyles.Any, CultureInfo.InvariantCulture);
                var z = float.Parse(posData[i * 3 + 2], NumberStyles.Any, CultureInfo.InvariantCulture);
                var position = new Vector4(x, y, z, 1);
                position = Vector4.Transform(position, Correction);
                Vertices.Add(new Vertex(Vertices.Count, position.Xyz(), Skinning.VerticesSkinData[Vertices.Count]));
            }
        }

        private void ReadNormals(XmlNode PolyNode)
        {
            var normalsId = PolyNode.ChildWithAttribute("input", "semantic", "NORMAL").GetAttribute("source").Value
                .Substring(1);
            XmlNode normalsData = MeshData.ChildWithAttribute("source", "id", normalsId)["float_array"];
            var count = int.Parse(normalsData.GetAttribute("count").Value);
            var normData = normalsData.InnerText.Split(' ');
            for (var i = 0; i < count / 3; i++)
            {
                var x = float.Parse(normData[i * 3], NumberStyles.Any, CultureInfo.InvariantCulture);
                var y = float.Parse(normData[i * 3 + 1], NumberStyles.Any, CultureInfo.InvariantCulture);
                var z = float.Parse(normData[i * 3 + 2], NumberStyles.Any, CultureInfo.InvariantCulture);
                var norm = new Vector4(x, y, z, 1);
                norm = Vector4.Transform(norm, Correction);
                Normals.Add(norm.Xyz());
            }
        }

        private void ReadColors(XmlNode PolyNode)
        {
            var colorsContainer = PolyNode.ChildWithAttribute("input", "semantic", "COLOR");
            if (colorsContainer == null)
                throw new ArgumentException(
                    "Model does not have any vertex colors, are you sure you applied the materials?");
            var colorsId = colorsContainer.GetAttribute("source").Value.Substring(1);
            XmlNode ColorsNode = MeshData.ChildWithAttribute("source", "id", colorsId)["float_array"];
            var count = int.Parse(ColorsNode.GetAttribute("count").Value);
            var ColorsData = ColorsNode.InnerText.Split(' ');
            for (var i = 0; i < count / 3; i++)
            {
                var s = float.Parse(ColorsData[i * 3 + 0], NumberStyles.Any, CultureInfo.InvariantCulture);
                var t = float.Parse(ColorsData[i * 3 + 1], NumberStyles.Any, CultureInfo.InvariantCulture);
                var q = float.Parse(ColorsData[i * 3 + 2], NumberStyles.Any, CultureInfo.InvariantCulture);
                Colors.Add(new Vector3(s, t, q));
            }
        }

        private void AssembleVertices(XmlNode PolyNode)
        {
            var typeCount = PolyNode.ChildrenCount("input");
            var indexData = PolyNode["p"].InnerText.Split(' ');
            for (var i = 0; i < indexData.Length / typeCount; i++)
            {
                var PositionIndex = int.Parse(indexData[i * typeCount]);
                var NormalIndex = int.Parse(indexData[i * typeCount + 1]);
                var TexCoordIndex = int.Parse(indexData[i * typeCount + 2]);

                ProcessVertex(PositionIndex, NormalIndex, TexCoordIndex);
            }
        }


        private Vertex ProcessVertex(int posIndex, int normIndex, int texIndex)
        {
            var CurrentVertex = Vertices[posIndex];

            if (!CurrentVertex.IsSet)
            {
                CurrentVertex.ColorIndex = texIndex;
                CurrentVertex.NormalIndex = normIndex;
                Indices.Add((uint)posIndex);
                return CurrentVertex;
            }

            return DealWithAlreadyProcessedVertex(CurrentVertex, texIndex, normIndex);
        }

        private float ConvertDataToArrays()
        {
            float furthestPoint = 0;
            for (var i = 0; i < Vertices.Count; i++)
            {
                var currentVertex = Vertices[i];
                if (currentVertex.Length > furthestPoint) furthestPoint = currentVertex.Length;
                var weights = currentVertex.WeightsData;
                JointIdsArray[i * 3] = weights.JointIds[0];
                JointIdsArray[i * 3 + 1] = weights.JointIds[1];
                JointIdsArray[i * 3 + 2] = weights.JointIds[2];
                WeightsArray[i * 3] = weights.Weights[0];
                WeightsArray[i * 3 + 1] = weights.Weights[1];
                WeightsArray[i * 3 + 2] = weights.Weights[2];
            }

            return furthestPoint;
        }

        private Vertex DealWithAlreadyProcessedVertex(Vertex previousVertex, int newTextureIndex, int newNormalIndex)
        {
            if (previousVertex.HasSameTextureAndNormal(newTextureIndex, newNormalIndex))
            {
                Indices.Add((uint)previousVertex.Index);
                return previousVertex;
            }

            var anotherVertex = previousVertex.DuplicateVertex;
            if (anotherVertex != null)
            {
                return DealWithAlreadyProcessedVertex(anotherVertex, newTextureIndex, newNormalIndex);
            }

            var duplicateVertex = new Vertex(Vertices.Count, previousVertex.Position, previousVertex.WeightsData);
            duplicateVertex.ColorIndex = newTextureIndex;
            duplicateVertex.NormalIndex = newNormalIndex;
            previousVertex.DuplicateVertex = duplicateVertex;
            Vertices.Add(duplicateVertex);
            Indices.Add((uint)duplicateVertex.Index);
            return duplicateVertex;
        }

        private void InitArrays()
        {
            JointIdsArray = new int[Vertices.Count * 3];
            WeightsArray = new float[Vertices.Count * 3];
        }

        private void RemoveUnusedVertices()
        {
            for (var i = 0; i < Vertices.Count; i++)
            {
                Vertices[i].AverageTangents();
                if (!Vertices[i].IsSet)
                {
                    Vertices[i].ColorIndex = 0;
                    Vertices[i].NormalIndex = 0;
                }
            }
        }
    }
}