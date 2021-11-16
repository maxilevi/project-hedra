using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hedra.Engine.Rendering.Core;
using Hedra.Rendering;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace Hedra.Engine.Rendering.UI
{
    public static class TextCache
    {
        private static readonly object Lock = new object();
        private static readonly List<CacheOptions> Cache = new List<CacheOptions>();

        public static int Count
        {
            get
            {
                lock (Lock)
                {
                    return Cache.Count;
                }
            }
        }

        private static CacheOptions GetCache(string Text, Font TextFont, Color TextColor)
        {
            lock (Lock)
            {
                return Cache.FirstOrDefault(C =>
                    C.Text == Text && Math.Abs(C.TextFont.Size - TextFont.Size) < 0.005f &&
                    C.TextFont.Style == TextFont.Style && TextColor == C.TextColor);
            }
        }

        private static bool Contains(string Text, Font TextFont, Color TextColor)
        {
            return GetCache(Text, TextFont, TextColor) != null;
        }

        public static bool Exists(uint Id)
        {
            lock (Lock)
            {
                return Cache.FirstOrDefault(C => C.Id == Id) != null;
            }
        }

        public static uint UseOrCreate(string Text, Font TextFont, Color TextColor, BitmapObject Bitmap, StackTrace _t)
        {
            var cache = GetCache(Text, TextFont, TextColor);
            var id = 0u;
            if (cache == null)
            {
                Add(Text, TextFont, TextColor, id = Graphics2D.LoadTexture(Bitmap, false), _t);
            }
            else
            {
                id = cache.Id;
                cache.Uses++;
            }

            return id;
        }

        private static void Add(string Text, Font TextFont, Color TextColor, uint Id, StackTrace T)
        {
            if (Contains(Text, TextFont, TextColor))
                throw new ArgumentOutOfRangeException($"Duplicate cache for '{Text}'");
            lock (Lock)
            {
                var cache = Cache.FirstOrDefault(C => C.Id == Id);
                if (cache != null) throw new ArgumentOutOfRangeException($"Duplicate cache for '{Text}'");
                if (Program.IsDummy) return;
                Cache.Add(new CacheOptions
                {
                    TextFont = TextFont,
                    Text = Text,
                    TextColor = TextColor,
                    Id = Id,
                    Uses = 1,
                    _stack = T
                });
            }
        }

        public static void Remove(uint Id)
        {
            if (Id == 0) return;
            lock (Lock)
            {
                var cache = Cache.FirstOrDefault(C => C.Id == Id);
                if (cache == null)
                    return; // throw new ArgumentOutOfRangeException($"Cache does not exist for id '{Id}'");
                if (cache.Text.Contains("FXAA"))
                {
                    var a = 0;
                }

                if (--cache.Uses == 0)
                {
                    Cache.Remove(cache);
                    TextureRegistry.Dispose(Id);
                }
            }
        }

        private class CacheOptions
        {
            public StackTrace _stack;

            public CacheOptions()
            {
#if DEBUG
                _stack = new StackTrace();
#endif
            }

            public string Text { get; set; }
            public Font TextFont { get; set; }
            public Color TextColor { get; set; }
            public uint Id { get; set; }
            public uint Uses { get; set; }

            public override string ToString()
            {
                return $"{Id}|{Text}|{Uses}";
            }
        }
    }
}