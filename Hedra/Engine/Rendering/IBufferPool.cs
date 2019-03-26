using System;
using System.Collections.Generic;
using System.Drawing;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.Rendering
{
    public interface IBufferPool
    {
        bool Remove(Vector2 Offset);
        bool Update(Vector2 Offset, VertexData Data);
        void Draw(Dictionary<Vector2, Chunk> ToDraw);
        void DrawShadows(Dictionary<Vector2, Chunk> ShadowDraw, ref IntPtr[] ShadowOffsets, ref int[] ShadowCounts);
        Bitmap Visualize();
        void Discard();
        void ForceDiscard();
        IComparer<KeyValuePair<Vector2, ChunkRenderCommand>> Comparer { set; }
        int TotalMemory { get; }
    }
}