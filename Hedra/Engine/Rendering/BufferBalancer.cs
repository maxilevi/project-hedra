using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Windowing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Hedra.Engine.Rendering
{
    public class BufferBalancer
    {
        private readonly WorldBuffer[] _buffers;

        public BufferBalancer(params WorldBuffer[] Buffers)
        {
            _buffers = Buffers;
        }

        public IComparer<KeyValuePair<Vector2, ChunkRenderCommand>> Comparer
        {
            set => Each(B => B.Comparer = value);
        }

        public long TotalMemory => _buffers.Sum(B => (long) B.TotalMemory);

        public bool Remove(Vector2 Offset)
        {
            var result = true;
            for (var i = 0; i < _buffers.Length; ++i) result &= _buffers[i].Remove(Offset);
            return result;
        }

        public bool Update(Vector2 Offset, NativeVertexData Data)
        {
            for (var i = 0; i < _buffers.Length; ++i)
                if (_buffers[i].Has(Offset))
                    return _buffers[i].Update(Offset, Data);
            return _buffers.OrderBy(B => 1f - B.AvailableMemory / (float)B.TotalMemory).First().Update(Offset, Data);
        }

        public void Draw(Dictionary<Vector2, Chunk> ToDraw)
        {
            Each(B =>
            {
                B.Bind();
                B.BindIndices();
                Renderer.MultiDrawElements(PrimitiveType.Triangles, B.Counts, DrawElementsType.UnsignedInt, B.Offsets,
                    B.BuildCounts(ToDraw));
                B.UnbindIndices();
                B.Unbind();
            });
        }

        public void DrawShadows(Dictionary<Vector2, Chunk> ShadowDraw, ref IntPtr[] ShadowOffsets,
            ref uint[] ShadowCounts)
        {
            for (var i = 0; i < _buffers.Length; ++i)
            {
                _buffers[i].Bind();
                Renderer.DisableVertexAttribArray(2);
                _buffers[i].BindIndices();
                var shadowCount = _buffers[i].BuildCounts(ShadowDraw, ref ShadowOffsets, ref ShadowCounts);
                Renderer.MultiDrawElements(PrimitiveType.Triangles, ShadowCounts, DrawElementsType.UnsignedInt,
                    ShadowOffsets, shadowCount);
                _buffers[i].UnbindIndices();
                _buffers[i].Unbind();
            }
        }

        public Image<Rgba32> Visualize()
        {
            var verticesBmp = _buffers.Where(B => B.Indices.Count == 0).Select(B => B.Vertices.Draw()).ToArray();
            var indicesBmp = _buffers.Where(B => B.Indices.Count != 0).Select(B => B.Indices.Draw()).ToArray();
            var allBmp = indicesBmp.Concat(verticesBmp).ToArray();
            const int padding = 16;
            var bmp = new Image<Rgba32>(allBmp.First().Width, allBmp.Sum(B => B.Height) + padding * allBmp.Length);
            bmp.Mutate(I =>
            {
                var accumulated = 0;
                for (var i = 0; i < allBmp.Length; ++i)
                {
                    I.DrawImage(allBmp[i], 0, accumulated);
                    accumulated += allBmp[i].Height + padding;
                }
            });

            return bmp;
        }

        public void Discard()
        {
            Each(B => B.Discard());
        }

        public void ForceDiscard()
        {
            Each(B => B.ForceDiscard());
        }

        private void Each(Action<WorldBuffer> Buffer)
        {
            for (var i = 0; i < _buffers.Length; ++i) Buffer(_buffers[i]);
        }
    }
}