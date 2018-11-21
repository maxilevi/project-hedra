/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:21 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using OpenTK;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    /// <summary>
    /// Description of MeshData.
    /// </summary>
    public class ModelData
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
            var offset = Model.Vertices.Length;
            var indexOffset = Model.Indices.Length;
            var newIndices = new uint[indexOffset + Models.Sum(M => M.Indices.Length)];
            for (var i = 0; i < Models.Length; i++)
            {
                for (var k = 0; k < Models[i].Indices.Length; ++k)
                {
                    newIndices[indexOffset] = (uint)(Models[i].Indices[k] + offset);
                }
                indexOffset += Models[i].Indices.Length;
                offset += Models[i].Vertices.Length;
            }
            return new ModelData(
                Model.Vertices.Concat(Models.SelectMany(M => M.Vertices)).ToArray(),
                Model.Colors.Concat(Models.SelectMany(M => M.Colors)).ToArray(),
                Model.Normals.Concat(Models.SelectMany(M => M.Normals)).ToArray(),
                newIndices,
                Model.JointIds.Concat(Models.SelectMany(M => M.JointIds)).ToArray(),
                Model.VertexWeights.Concat(Models.SelectMany(M => M.VertexWeights)).ToArray()
            );
        }
    }
}
