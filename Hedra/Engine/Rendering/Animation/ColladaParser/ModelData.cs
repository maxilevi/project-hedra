/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:21 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hedra.Engine.Rendering.Geometry;
using Hedra.Framework;
using Hedra.Rendering;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    /// <summary>
    ///     Description of MeshData.
    /// </summary>
    public class ModelData : IModelData
    {
        private const int DefaultStitchJointId = -1;
        public ModelData(Vector3[] Vertices, Vector3[] Colors, Vector3[] Normals, uint[] Indices, Vector3[] JointIds,
            Vector3[] VertexWeights, string[] JointNames)
        {
            this.Vertices = Vertices;
            this.Colors = Colors;
            this.Normals = Normals;
            this.Indices = Indices;
            this.JointIds = JointIds;
            this.VertexWeights = VertexWeights;
            this.JointNames = JointNames;
        }

        public Vector3[] JointIds { get; }
        public Vector3[] VertexWeights { get; }
        public Vector3[] Vertices { get; }
        public Vector3[] Colors { get; }
        public Vector3[] Normals { get; }
        public string[] JointNames { get; }
        public string Name { get; set; }

        public static ModelData Empty { get; } = new (Array.Empty<Vector3>(), Array.Empty<Vector3>(), Array.Empty<Vector3>(),
            Array.Empty<uint>(), Array.Empty<Vector3>(), Array.Empty<Vector3>(), Array.Empty<string>());

        public uint[] Indices { get; }

        public void Paint(Vector3 Color, Vector3 Replacement)
        {
            for (var i = 0; i < Colors.Length; ++i)
                if (Colors[i] == Color)
                    Colors[i] = Replacement;
        }

        public void Transform(Matrix4x4 Transformation)
        {
            MeshOperations.Transform(Vertices, Normals, Transformation);
        }

        public VertexData ToVertexData()
        {
            return new VertexData
            {
                Indices = Indices.ToList(),
                Normals = Normals.ToList(),
                Vertices = Vertices.ToList(),
                Colors = Colors.Select(C => new Vector4(C, 1)).ToList()
            };
        }

        private static bool JointNameHas(string[] Names, Vector3 Ids, string Needle, out string Name)
        {
            var nameX = Names[(int)Ids.X].Contains(Needle);
            var nameY = Names[(int)Ids.Y].Contains(Needle);
            var nameZ = Names[(int)Ids.Z].Contains(Needle);
            Name = Names[(int)Ids.X];
            if (nameX) return true;
            Name = Names[(int)Ids.Y];
            if (nameY) return true;
            Name = Names[(int)Ids.Z];
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
                    if (!localMap.ContainsKey(name))
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
                    if (!map.ContainsKey(pair.Key))
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
                for (var h = 0; h < parts[i].Length; ++h)
                {
                    var v1 = parts[i][h].Position;
                    var minDist = float.MaxValue;
                    var nearestModel = -1;
                    var nearestIndex = -1;
                    var others = parts.Where(P => P != parts[i]).ToList();
                    for (var j = 0; j < others.Count; ++j)
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

                    if (nearestIndex == -1) continue;
                    pairsToMatch.Add(
                        new Pair<Vector3, StitchVertex>(
                            parts[i][h].Position,
                            others[nearestModel][nearestIndex]
                        )
                    );
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
                var a = 0;
                return Index == 0 ? Vector3.UnitX : Index == 1 ? Vector3.UnitY : Vector3.UnitZ;
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
                    newIndices[indexOffset + k] = (uint)(Models[i].Indices[k] + offset);
                indexOffset += Models[i].Indices.Length;
                offset += Models[i].Vertices.Length;
            }

            for (var i = 0; i < Model.Indices.Length; i++)
                newIndices[i + indexOffset] = (uint)(Model.Indices[i] + offset);

            return newIndices;
        }

        private static void MergeJoints(ModelData Model, ModelData[] Models, out string[][] JointIdsAsNames)
        {
            JointIdsAsNames = Models.SelectMany(
                M => M.JointIds.Select(X => JointIdsToNames(X, M.JointNames))).Concat(
                Model.JointIds.Select(X => JointIdsToNames(X, Model.JointNames))
            ).ToArray();
        }

        private static Vector3 JointNamesToIds(string[] Names, Dictionary<string, int> Map)
        {
            return new Vector3(
                Names[0] != null ? Map[Names[0]] : 0,
                Names[1] != null ? Map[Names[1]] : 0,
                Names[2] != null ? Map[Names[2]] : 0
            );
        }

        public static ModelData Combine(Dictionary<string, int> NameToJointId, ModelData Model, params ModelData[] Models)
        {
            var pairsToMatch = CalculatePairsToMatch(Models);
            var newIndices = MergeIndices(Model, Models);
            MergeJoints(Model, Models, out var jointIdsAsNames);

            var final = new ModelData(
                Models.SelectMany(M => M.Vertices).Concat(Model.Vertices).ToArray(),
                Models.SelectMany(M => M.Colors).Concat(Model.Colors).ToArray(),
                Models.SelectMany(M => M.Normals).Concat(Model.Normals).ToArray(),
                newIndices,
                new Vector3[jointIdsAsNames.Length],
                Models.SelectMany(M => M.VertexWeights).Concat(Model.VertexWeights).ToArray(),
                NameToJointId.Keys.ToArray()
            );

            for (var j = 0; j < final.Vertices.Length; ++j)
            {
                var output = ClearStitchData(jointIdsAsNames[j], final.VertexWeights[j]);
                final.JointIds[j] = JointNamesToIds(output.One, NameToJointId);
                final.VertexWeights[j] = output.Two;
            }

            for (var j = 0; j < final.Vertices.Length; ++j)
            {
                for (var i = pairsToMatch.Count - 1; i > -1; --i)
                {
                    if (final.Vertices[j] != pairsToMatch[i].One) continue;

                    var output = ClearStitchData(pairsToMatch[i].Two.JointIds, pairsToMatch[i].Two.Weights);

                    final.Vertices[j] = pairsToMatch[i].Two.Position;
                    final.JointIds[j] = JointNamesToIds(output.One, NameToJointId);
                    final.VertexWeights[j] = output.Two;
                }
            }

            var freqs = final.JointIds.SelectMany(x => new[] { x.X, x.Y, x.Z }).GroupBy(x => x)
                .ToDictionary(x => x.Key, x => x.Count());
            for (var i = 0; i < final.JointNames.Length; i++)
            {
                if (!final.JointNames[i].Contains("ST")) continue;
                if (freqs.ContainsKey(i))
                {
                    var a = 0;
                }
            }

            for (var i = 0; i < final.JointIds.Length; i++)
                if (final.JointIds[i] == Vector3.Zero)
                {
                    var a = 0;
                }

            for (var i = 0; i < final.VertexWeights.Length; i++)
            {
                if (final.VertexWeights[i] == Vector3.Zero)
                {
                    var a = 0;
                }

                if (Math.Abs(final.VertexWeights[i].X + final.VertexWeights[i].Y + final.VertexWeights[i].Z) < 0.005f)
                {
                    var a = 0;
                }
            }

            return final;
        }

        private static Pair<string[], Vector3> ClearStitchData(string[] JointIds, Vector3 VertexWeights)
        {
            var nameX = JointIds[0];
            var nameY = JointIds[1];
            var nameZ = JointIds[2];

            var ids = new []{nameX, nameY, nameZ};
            var weights = VertexWeights;
            var cleared = 0;
            if (nameX.Contains("ST"))
            {
                ids[0] = null;
                weights = DistributeWeight(weights, 0);
                cleared++;
            }

            if (nameY.Contains("ST"))
            {
                ids[1] = null;
                weights = DistributeWeight(weights, 1);
                cleared++;
            }

            if (nameZ.Contains("ST"))
            {
                ids[2] = null;
                weights = DistributeWeight(weights, 2);
                cleared++;
            }


            if (cleared > 1 || Math.Abs(weights.X + weights.Y + weights.Z - 1) > 0.1f )
            {
                int a = 0;
            }
            return new Pair<string[], Vector3>(ids, weights);
        }

        public ModelData Clone()
        {
            var model = new ModelData(Vertices.ToArray(), Colors.ToArray(), Normals.ToArray(), Indices.ToArray(),
                JointIds.ToArray(), VertexWeights.ToArray(), JointNames.ToArray())
            {
                Name = Name
            };
            return model;
        }

        private struct StitchVertex
        {
            public readonly Vector3 Position;
            public readonly Vector3 Weights;
            public readonly string[] JointIds;
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