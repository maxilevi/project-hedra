using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Hedra.Engine.Rendering.Core;
using Hedra.Rendering;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace Hedra.Engine.Rendering.UI
{
    public static class TextCache
    {
        private static readonly object Lock = new object();
        private static readonly Dictionary<TextRenderKey, CacheEntry> Cache = new Dictionary<TextRenderKey, CacheEntry>();
        private static readonly Dictionary<uint, CacheEntry> ById = new Dictionary<uint, CacheEntry>();

        public static int Count
        {
            get { lock (Lock) return Cache.Count; }
        }

        public static bool Exists(uint Id)
        {
            lock (Lock) return ById.ContainsKey(Id);
        }

        public static bool TryUse(string Text, Font Font, Color Color, TextOptions Options, float RasterScale,
            out uint Id, out Vector2 PixelSize)
        {
            return TryUse(Text, Font, Color, Options.HasStroke, Options.StrokeWidth, Options.StrokeColor,
                RasterScale, out Id, out PixelSize);
        }

        public static bool TryUse(string Text, Font Font, Color Color, bool Stroke, float StrokeWidth,
            Color StrokeColor, float RasterScale, out uint Id, out Vector2 PixelSize)
        {
            lock (Lock)
            {
                if (Cache.TryGetValue(new TextRenderKey(Text, Font, Color, Stroke, StrokeWidth, StrokeColor, RasterScale), out var entry))
                {
                    entry.Uses++;
                    Id = entry.Id;
                    PixelSize = entry.PixelSize;
                    return true;
                }
                Id = 0;
                PixelSize = Vector2.Zero;
                return false;
            }
        }

        public static uint UseOrCreate(string Text, Font Font, Color Color, TextOptions Options, float RasterScale,
            BitmapObject Bitmap, out Vector2 PixelSize)
        {
            lock (Lock)
            {
                var key = new TextRenderKey(Text, Font, Color, Options, RasterScale);
                if (Cache.TryGetValue(key, out var entry))
                {
                    entry.Uses++;
                    PixelSize = entry.PixelSize;
                    return entry.Id;
                }

                PixelSize = new Vector2(Bitmap.Bitmap.Width, Bitmap.Bitmap.Height);
                var id = Graphics2D.LoadTexture(Bitmap, false);
                if (id == 0 || Program.IsDummy) return id;
                entry = new CacheEntry(key, id, PixelSize);
                Cache.Add(key, entry);
                ById.Add(id, entry);
                return id;
            }
        }

        // Retain the existing entry point for callers supplying their own bitmap.
        public static uint UseOrCreate(string Text, Font TextFont, Color TextColor, BitmapObject Bitmap, StackTrace _t)
        {
            return UseOrCreate(Text, TextFont, TextColor, new TextOptions(), TextProvider.RenderingScale, Bitmap, out _);
        }

        public static void Remove(uint Id)
        {
            if (Id == 0) return;
            lock (Lock)
            {
                if (!ById.TryGetValue(Id, out var entry) || --entry.Uses != 0) return;
                Cache.Remove(entry.Key);
                ById.Remove(Id);
                TextureRegistry.Dispose(Id);
            }
        }

        private sealed class CacheEntry
        {
            public CacheEntry(TextRenderKey Key, uint Id, Vector2 PixelSize)
            {
                this.Key = Key;
                this.Id = Id;
                this.PixelSize = PixelSize;
                Uses = 1;
            }

            public TextRenderKey Key { get; }
            public uint Id { get; }
            public Vector2 PixelSize { get; }
            public int Uses { get; set; }
        }
    }
}
