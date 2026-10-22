using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Hedra.Engine.Rendering.Core;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace Hedra.Engine.Rendering.UI
{
    public static class TextCache
    {
        private static readonly object Lock = new object();
        private static readonly Dictionary<CacheKey, CacheEntry> Cache = new Dictionary<CacheKey, CacheEntry>();
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
            lock (Lock)
            {
                if (Cache.TryGetValue(new CacheKey(Text, Font, Color, Options, RasterScale), out var entry))
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
                var key = new CacheKey(Text, Font, Color, Options, RasterScale);
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

        private readonly struct CacheKey : IEquatable<CacheKey>
        {
            private readonly string _text;
            private readonly FontEntry _font;
            private readonly Color _color;
            private readonly bool _stroke;
            private readonly float _strokeWidth;
            private readonly Color _strokeColor;
            private readonly float _rasterScale;

            public CacheKey(string Text, Font Font, Color Color, TextOptions Options, float RasterScale)
            {
                _text = Text;
                _font = new FontEntry(Font.Family, Font.Size, Font.Instance.Description.Style);
                _color = Color;
                _stroke = Options.HasStroke;
                _strokeWidth = Options.StrokeWidth;
                _strokeColor = Options.StrokeColor;
                _rasterScale = RasterScale;
            }

            public bool Equals(CacheKey Other)
            {
                return _text == Other._text && _font.Equals(Other._font) && _color.Equals(Other._color) &&
                       _stroke == Other._stroke && _strokeWidth.Equals(Other._strokeWidth) &&
                       _strokeColor.Equals(Other._strokeColor) && _rasterScale.Equals(Other._rasterScale);
            }

            public override bool Equals(object Obj) => Obj is CacheKey other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(_text, _font, _color, _stroke,
                _strokeWidth, _strokeColor, _rasterScale);
        }

        private sealed class CacheEntry
        {
            public CacheEntry(CacheKey Key, uint Id, Vector2 PixelSize)
            {
                this.Key = Key;
                this.Id = Id;
                this.PixelSize = PixelSize;
                Uses = 1;
            }

            public CacheKey Key { get; }
            public uint Id { get; }
            public Vector2 PixelSize { get; }
            public int Uses { get; set; }
        }
    }
}
