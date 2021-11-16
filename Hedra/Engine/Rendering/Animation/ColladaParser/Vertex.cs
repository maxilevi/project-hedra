/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:22 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Collections.Generic;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    /// <summary>
    ///     Description of Vertex.
    /// </summary>
    public class Vertex
    {
        private const int NoIndex = -1;
        private readonly List<Vector3> Tangents = new List<Vector3>();

        public Vertex(int index, Vector3 Position, VertexSkinData weightsData)
        {
            ColorIndex = NoIndex;
            NormalIndex = NoIndex;

            Index = index;
            WeightsData = weightsData;
            this.Position = Position;
            Length = Position.Length();
        }

        public Vector3 Position { get; }
        public int ColorIndex { get; set; }
        public int NormalIndex { get; set; }
        public Vertex DuplicateVertex { get; set; }
        public int Index { get; }
        public float Length { get; }
        public Vector3 AveragedTangent { get; private set; }

        public VertexSkinData WeightsData { get; }

        public bool IsSet => ColorIndex != NoIndex && NormalIndex != NoIndex;

        public void AddTangent(Vector3 tangent)
        {
            Tangents.Add(tangent);
        }

        public void AverageTangents()
        {
            if (Tangents.Count == 0) return;
            for (var i = 0; i < Tangents.Count; i++) AveragedTangent += Tangents[i];
            AveragedTangent = AveragedTangent.Normalized();
        }

        public bool HasSameTextureAndNormal(int textureIndexOther, int normalIndexOther)
        {
            return textureIndexOther == ColorIndex && normalIndexOther == NormalIndex;
        }
    }
}