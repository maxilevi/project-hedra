using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Diagnostics;

namespace Hedra.Engine.Rendering.UI
{
    public static class TextCache
    {
        private static readonly List<CacheOptions> Cache = new List<CacheOptions>();
        
        private static CacheOptions GetCache(string Text, Font TextFont, Color TextColor)
        {
            return Cache.FirstOrDefault(C => C.Text == Text && Math.Abs(C.TextFont.Size - TextFont.Size) < 0.005f && C.TextFont.Style == TextFont.Style && TextColor == C.TextColor);
        }

        private static bool Contains(string Text, Font TextFont, Color TextColor)
        {
            return GetCache(Text, TextFont, TextColor) != null;
        }

        public static bool Exists(uint Id)
        {
            return Cache.FirstOrDefault(C => C.Id == Id) != null;
        }
        
        public static uint UseOrCreate(string Text, Font TextFont, Color TextColor, BitmapObject Bitmap, StackTrace _t)
        {
            var cache = GetCache(Text, TextFont, TextColor);
            var id = 0u;
            if (cache == null)
            {
                Add(Text, TextFont, TextColor, id = Graphics2D.LoadTexture(Bitmap), _t);
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
            if (Contains(Text, TextFont, TextColor)) throw new ArgumentOutOfRangeException($"Duplicate cache for '{Text}'");
            var cache = Cache.FirstOrDefault(C => C.Id == Id);
            if (cache != null) throw new ArgumentOutOfRangeException($"Duplicate cache for '{Text}'");
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

        public static void Remove(uint Id)
        {
            if(Id == 0) return;
            var cache = Cache.FirstOrDefault(C => C.Id == Id);
            if (cache == null) return;// throw new ArgumentOutOfRangeException($"Cache does not exist for id '{Id}'");
            if ((--cache.Uses) == 0)
                Cache.Remove(cache);
        }

        public static int Count => Cache.Count;
        
        private class CacheOptions
        {
            public string Text { get; set; }
            public Font TextFont { get; set; }
            public Color TextColor { get; set; }
            public uint Id { get; set; }
            public uint Uses { get; set; }
            public StackTrace _stack = new StackTrace();

            public override string ToString()
            {
                return $"{Id}|{Text}|{Uses}";
            }
        }
    }
}