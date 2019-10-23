/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:21 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System.Numerics;
using System;
using System.Linq;
using System.Collections.Generic;
using Hedra.Rendering;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    /// <summary>
    /// Description of MeshData.
    /// </summary>
    public class ModelData : IModelData
    {
        public Vector3[] JointIds { get; }
        public Vector3[] VertexWeights { get; }
        public Vector3[] Vertices { get; }
        public Vector3[] Colors { get; }
        public Vector3[] Normals { get; }
        public uint[] Indices { get; }
    
        public ModelData(Vector3[] Vertices, Vector3[] Colors, Vector3[] Normals, uint[] Indices, Vector3[] JointIds, Vector3[] VertexWeights)
        {
            this.Vertices = Vertices;
            this.Colors = Colors;
            this.Normals = Normals;
            this.Indices = Indices;
            this.JointIds = JointIds;
            this.VertexWeights = VertexWeights;
        }

        public VertexData ToVertexData()
        {
            return new VertexData
            {
                Indices = Indices.ToList(),
                Normals = Normals.ToList(),
                Vertices = Vertices.ToList(),
                Colors = Colors.Select(C => new Vector4(C, 1)).ToList(),
            };
        }
        
        public static ModelData Combine(ModelData Model, params ModelData[] Models)
        {
            var offset = 0;
            var indexOffset = 0;
            var newIndices = new uint[Model.Indices.Length + Models.Sum(M => M.Indices.Length)];
            for (var i = 0; i < Models.Length; i++)
            {
                for (var k = 0; k < Models[i].Indices.Length; ++k)
                {
                    newIndices[indexOffset + k] = (uint)(Models[i].Indices[k] + offset);
                }
                indexOffset += Models[i].Indices.Length;
                offset += Models[i].Vertices.Length;
            }
            for (var i = 0; i < Model.Indices.Length; i++)
            {
                newIndices[i + indexOffset] = (uint)(Model.Indices[i] + offset);
            }
            return new ModelData(
                Models.SelectMany(M => M.Vertices).Concat(Model.Vertices).ToArray(),
                Models.SelectMany(M => M.Colors).Concat(Model.Colors).ToArray(),
                Models.SelectMany(M => M.Normals).Concat(Model.Normals).ToArray(),
                newIndices,
                Models.SelectMany(M => M.JointIds).Concat(Model.JointIds).ToArray(),
                Models.SelectMany(M => M.VertexWeights).Concat(Model.VertexWeights).ToArray()
            );
        }

        public static ModelData Empty { get; } = new ModelData(new Vector3[0], new Vector3[0], new Vector3[0], new uint[0], new Vector3[0], new Vector3[0]);
    }
}
