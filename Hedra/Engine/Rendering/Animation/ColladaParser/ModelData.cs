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
using Hedra.Framework;
using Hedra.Numerics;
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
        public string[] JointNames { get; }
    
        public ModelData(Vector3[] Vertices, Vector3[] Colors, Vector3[] Normals, uint[] Indices, Vector3[] JointIds, Vector3[] VertexWeights, string[] JointNames)
        {
            this.Vertices = Vertices;
            this.Colors = Colors;
            this.Normals = Normals;
            this.Indices = Indices;
            this.JointIds = JointIds;
            this.VertexWeights = VertexWeights;
            this.JointNames = JointNames;
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

        private static bool JointNameHas(string[] Names, Vector3 Ids, string Needle, out string Name)
        {
            var nameX = Names[(int) Ids.X].Contains(Needle);
            var nameY = Names[(int) Ids.Y].Contains(Needle);
            var nameZ = Names[(int) Ids.Z].Contains(Needle);
            Name = Names[(int) Ids.X];
            if (nameX) return true;
            Name = Names[(int) Ids.Y];
            if (nameY) return true;
            Name = Names[(int) Ids.Z];
            return nameZ;
        }

        private static Dictionary<string, List<StitchVertex[]>> GetMatchableVertexGroups(ModelData[] Models)
        {
            var map = new Dictionary<string, List<StitchVertex[]>>();
            for (var i = 0; i < Models.Length; ++i)
            {
                var localMap = new Dictionary<string, List<StitchVertex>>();
                for (var j = 0; j < Models[i].Vertices.Length; ++j)
                {
                    if (!JointNameHas(Models[i].JointNames, Models[i].JointIds[j], "ST", out var name)) continue;
                    if(!localMap.ContainsKey(name))
                        localMap.Add(name, new List<StitchVertex>());
                    localMap[name].Add(
                        new StitchVertex(
                            Models[i].Vertices[j],
                            Models[i].VertexWeights[j],
                            Models[i].JointIds[j]
                        )
                    );
                }

                foreach (var pair in localMap)
                {
                    if(!map.ContainsKey(pair.Key))
                        map.Add(pair.Key, new List<StitchVertex[]>());
                    map[pair.Key].Add(pair.Value.ToArray());
                }
            }

            return map;
        }

        private static List<Pair<Vector3, StitchVertex>> CalculatePairsToMatch(ModelData[] Models)
        {
            var matchableVertices = GetMatchableVertexGroups(Models);
            var pairsToMatch = new List<Pair<Vector3, StitchVertex>>();
            foreach (var pair in matchableVertices)
            {
                for (var i = 0; i < pair.Value.Count; ++i)
                {
                    for (var h = 0; h < pair.Value[i].Length; ++h)
                    {
                        var minDist = float.MaxValue;
                        var nearestModel = -1;
                        var nearestIndex = -1;
                        for (var j = i + 1; j < pair.Value.Count; ++j)
                        {
                            for (var k = 0; k < pair.Value[j].Length; ++k)
                            {
                                var v1 = pair.Value[i][h].Position;
                                var v2 = pair.Value[j][k].Position;
                                var dist = (v1 - v2).LengthFast();
                                if (dist < minDist)
                                {
                                    minDist = dist;
                                    nearestModel = j;
                                    nearestIndex = k;
                                }
                            }
                        }
                        if(nearestIndex == -1) continue;
                        pairsToMatch.Add(
                            new Pair<Vector3, StitchVertex>(
                                pair.Value[i][h].Position,
                                pair.Value[nearestModel][nearestIndex]
                            )
                        );
                    }
                }
            }

            return pairsToMatch;
        }

        private static Vector3 DistributeWeight(Vector3 Weight, int Index)
        {
            var newWeight = new Vector3();
            
            if (Index == 0)
            {
                if (Weight.Y > 0 && Weight.Z > 0)
                {
                    var half = Weight.X / 2f;
                    newWeight = new Vector3(0, Weight.Y + half, Weight.Z + half);
                }
                else if (Weight.Y > 0)
                {
                    newWeight = new Vector3(0, 1, 0);
                } 
                else if (Weight.Z > 0)
                {
                    newWeight = new Vector3(0, 0, 1);
                }
            } 
            else if (Index == 1)
            {
                if (Weight.X > 0 && Weight.Z > 0)
                {
                    var half = Weight.Y / 2f;
                    newWeight = new Vector3(Weight.X + half, 0, Weight.Z + half);
                }
                else if (Weight.X > 0)
                {
                    newWeight = new Vector3(1, 0, 0);
                } 
                else if (Weight.Z > 0)
                {
                    newWeight = new Vector3(0, 0, 1);
                }
            } 
            else if (Index == 2)
            {
                if (Weight.X > 0 && Weight.Y > 0)
                {
                    var half = Weight.Z / 2f;
                    newWeight = new Vector3(Weight.X + half, Weight.Y + half, 0);
                }
                else if (Weight.X > 0)
                {
                    newWeight = new Vector3(1, 0, 0);
                } 
                else if (Weight.Y > 0)
                {
                    newWeight = new Vector3(0, 1, 0);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }

            if (newWeight == Vector3.Zero)
            {
                int a = 0;
                return Index == 0 ? Vector3.UnitX : (Index == 1 ? Vector3.UnitY : Vector3.UnitZ);
            }
            return newWeight;
        }
        
        public static ModelData Combine(ModelData Model, params ModelData[] Models)
        {
            var pairsToMatch = CalculatePairsToMatch(Models);
            
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
            
            var final = new ModelData(
                Models.SelectMany(M => M.Vertices).Concat(Model.Vertices).ToArray(),
                Models.SelectMany(M => M.Colors).Concat(Model.Colors).ToArray(),
                Models.SelectMany(M => M.Normals).Concat(Model.Normals).ToArray(),
                newIndices,
                Models.SelectMany(M => M.JointIds).Concat(Model.JointIds).ToArray(),
                Models.SelectMany(M => M.VertexWeights).Concat(Model.VertexWeights).ToArray(),
                new HashSet<string>(Models.SelectMany(M => M.JointNames).Concat(Model.JointNames).ToArray()).ToArray()
            );
            

            for (var j = 0; j < final.Vertices.Length; ++j)
            {
                var output = ClearStitchData(final.JointNames, final.JointIds[j], final.VertexWeights[j]);
                final.JointIds[j] = output.One;
                final.VertexWeights[j] = output.Two;

                for (var i = pairsToMatch.Count-1; i > -1; --i)
                {
                    if (final.Vertices[j] != pairsToMatch[i].One) continue;
                    
                    output = ClearStitchData(final.JointNames, pairsToMatch[i].Two.JointIds, pairsToMatch[i].Two.Weights);
                    
                    final.Vertices[j] = pairsToMatch[i].Two.Position;
                    final.JointIds[j] = output.One;
                    final.VertexWeights[j] = output.Two;
                    pairsToMatch.RemoveAt(i);
                }
            }

            return final;
        }

        private static Pair<Vector3, Vector3> ClearStitchData(string[] JointNames, Vector3 JointIds, Vector3 VertexWeights)
        {
            var nameX = JointNames[(int) JointIds.X];
            var nameY = JointNames[(int) JointIds.Y];
            var nameZ = JointNames[(int) JointIds.Z];
            
            var ids = JointIds;
            var weights = VertexWeights;
            if (nameX.Contains("ST"))
            {
                ids.X = 0;
                weights = DistributeWeight(weights, 0);
            }

            if (nameY.Contains("ST"))
            {
                ids.Y = 0;
                weights = DistributeWeight(weights, 1);
            }

            if (nameZ.Contains("ST"))
            {
                ids.Z = 0;
                weights = DistributeWeight(weights, 2);
            }
            return new Pair<Vector3, Vector3>(ids, weights);
        }

        public static ModelData Empty { get; } = new ModelData(new Vector3[0], new Vector3[0], new Vector3[0], new uint[0], new Vector3[0], new Vector3[0], new string[0]);

        struct StitchVertex
        {
            public Vector3 Position;
            public Vector3 Weights;
            public Vector3 JointIds;

            public StitchVertex(Vector3 Position, Vector3 Weights, Vector3 JointIds)
            {
                this.Position = Position;
                this.Weights = Weights;
                this.JointIds = JointIds;
            }
        }
    }
}
