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
        public string Name { get; set; }
    
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

        private static string[] JointIdsToNames(Vector3 Ids, string[] Names)
        {
            return new[]
            {
                Names[(int)Ids.X],
                Names[(int)Ids.Y],
                Names[(int)Ids.Z]
            };
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
                            Models[i].Name,
                            Models[i].Vertices[j],
                            Models[i].VertexWeights[j],
                            JointIdsToNames(Models[i].JointIds[j], Models[i].JointNames)
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
                var parts = pair.Value.OrderByDescending(X => X.Length).ToList();
                for (var i = 0; i < parts.Count; ++i)
                {
                    for (var h = 0; h < parts[i].Length; ++h)
                    {
                        var v1 = parts[i][h].Position;
                        var minDist = float.MaxValue;
                        var nearestModel = -1;
                        var nearestIndex = -1;
                        var others = parts.Where(P => P != parts[i]).ToList(); 
                        for (var j = 0; j < others.Count; ++j)
                        {
                            for (var k = 0; k < others[j].Length; ++k)
                            {
                                var v2 = others[j][k].Position;
                                var dist = (v1 - v2).LengthSquared();
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
                                parts[i][h].Position,
                                others[nearestModel][nearestIndex]
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

        private static uint[] MergeIndices(ModelData Model, ModelData[] Models)
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

            return newIndices;
        }
        
        private static Vector3[] MapJointIds(Vector3[] SourceIds, string[] SourceNames, Dictionary<string, int> Map)
        {
            var newIds = new Vector3[SourceIds.Length];
            for (var i = 0; i < SourceIds.Length; ++i)
            {
                newIds[i].X = Map[SourceNames[(int)SourceIds[i].X]];
                newIds[i].Y = Map[SourceNames[(int)SourceIds[i].Y]];
                newIds[i].Z = Map[SourceNames[(int)SourceIds[i].Z]];
            }

            return newIds;
        }

        private static void MergeJoints(ModelData Model, ModelData[] Models, out Vector3[] JointIds, out string[] JointNames, out Dictionary<string, int> Map)
        {
            JointNames =
                new HashSet<string>(Models.SelectMany(M => M.JointNames).Concat(Model.JointNames).ToArray()).ToArray();
            
            Map = new Dictionary<string, int>();
            for (var i = 0; i < JointNames.Length; ++i)
            {
                Map.Add(JointNames[i], i);
            }

            var map = Map;

            JointIds = Models.SelectMany(
                M => MapJointIds(M.JointIds, M.JointNames, map)).Concat(
                MapJointIds(Model.JointIds, Model.JointNames, map)
            ).ToArray();
        }
        
        public static ModelData Combine(ModelData Model, params ModelData[] Models)
        {
            var pairsToMatch = CalculatePairsToMatch(Models);
            var newIndices = MergeIndices(Model, Models);
            MergeJoints(Model, Models, out var jointIds, out var jointNames, out var jointNameToId);
            
            var final = new ModelData(
                Models.SelectMany(M => M.Vertices).Concat(Model.Vertices).ToArray(),
                Models.SelectMany(M => M.Colors).Concat(Model.Colors).ToArray(),
                Models.SelectMany(M => M.Normals).Concat(Model.Normals).ToArray(),
                newIndices,
                jointIds,
                Models.SelectMany(M => M.VertexWeights).Concat(Model.VertexWeights).ToArray(),
                jointNames
            );
            
            for (var j = 0; j < final.Vertices.Length; ++j)
            {
                var output = ClearStitchData(JointIdsToNames(final.JointIds[j], final.JointNames), final.VertexWeights[j], jointNameToId);
                final.JointIds[j] = output.One;
                final.VertexWeights[j] = output.Two;
            }

            for (var j = 0; j < final.Vertices.Length; ++j)
            {
                for (var i = pairsToMatch.Count-1; i > -1; --i)
                {
                    if (final.Vertices[j] != pairsToMatch[i].One) continue;
                    
                    var output = ClearStitchData(pairsToMatch[i].Two.JointIds, pairsToMatch[i].Two.Weights, jointNameToId);
                    
                    final.Vertices[j] = pairsToMatch[i].Two.Position;
                    final.JointIds[j] = output.One;
                    final.VertexWeights[j] = output.Two;
                }
            }
            
            var freqs = final.JointIds.SelectMany(x => new float[]{x.X, x.Y, x.Z}).GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
            for (var i = 0; i < final.JointNames.Length; i++)
            {
                if (!final.JointNames[i].Contains("ST")) continue;
                if (freqs.ContainsKey(i))
                {
                    int a = 0;
                }
            }

            for (var i = 0; i < final.JointIds.Length; i++)
            {
                if (final.JointIds[i] == Vector3.Zero)
                {
                    int a = 0;
                }
            }
            
            for (var i = 0; i < final.VertexWeights.Length; i++)
            {
                if (final.VertexWeights[i] == Vector3.Zero)
                {
                    int a = 0;
                }
                
                if (Math.Abs((final.VertexWeights[i].X + final.VertexWeights[i].Y + final.VertexWeights[i].Z)) < 0.005f)
                {
                    int a = 0;
                }
            }

            return final;
        }

        private static Pair<Vector3, Vector3> ClearStitchData(string[] JointIds, Vector3 VertexWeights, Dictionary<string, int> JointNameToId)
        {
            var nameX = JointIds[0];
            var nameY = JointIds[1];
            var nameZ = JointIds[2];
            
            var ids = new Vector3(JointNameToId[nameX], JointNameToId[nameY], JointNameToId[nameZ]);
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

            if (ids == Vector3.Zero)
            {
                int a = 0;
            }
            return new Pair<Vector3, Vector3>(ids, weights);
        }

        public static ModelData Empty { get; } = new ModelData(new Vector3[0], new Vector3[0], new Vector3[0], new uint[0], new Vector3[0], new Vector3[0], new string[0]);

        struct StitchVertex
        {
            public Vector3 Position;
            public Vector3 Weights;
            public string[] JointIds;
            public string Name;

            public StitchVertex(string Name, Vector3 Position, Vector3 Weights, string[] JointIds)
            {
                this.Name = Name;
                this.Position = Position;
                this.Weights = Weights;
                this.JointIds = JointIds;
            }
        }
    }
}
