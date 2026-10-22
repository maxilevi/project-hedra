using System;
using System.Collections.Generic;
using System.Linq;
using SixLabors.Fonts;

namespace Hedra.Rendering.UI
{
    public static class FontCache
    {
        private static readonly Dictionary<FontEntry, Font> CachedFonts = new Dictionary<FontEntry, Font>();
        private static FontFamily _normalFamily;
        private static FontFamily _boldFamily;

        static FontCache()
        {
            var defaultFamily = SystemFonts.Families.First(F => F.AvailableStyles.Contains(FontStyle.Bold));
            if (!SystemFonts.TryFind("Arial", out _normalFamily))
                _normalFamily = defaultFamily;
            if (!SystemFonts.TryFind("Arial", out _boldFamily))
                _boldFamily = defaultFamily;
        }   

        public static Font Default => Get(_normalFamily, 10);

        public static int Count
        {
            get { lock (CachedFonts) return CachedFonts.Count; }
        }

        public static void SetFonts(FontFamily NormalCollection, FontFamily BoldCollection)
        {
            lock (CachedFonts)
            {
                _normalFamily = NormalCollection;
                _boldFamily = BoldCollection;
            }
        }

        public static Font Get(Font Original, float Size)
        {
            return Get(Original.Family, Size, Original.Instance.Description.Style);
        }

        private static Font Get(FontFamily Family, float Size, FontStyle Style = FontStyle.Regular)
        {
            lock (CachedFonts)
            {
                var entry = new FontEntry(Family, Size, Style);
                if (!CachedFonts.TryGetValue(entry, out var font))
                {
                    font = new Font(entry.Family, entry.Size, entry.Style);
                    CachedFonts.Add(entry, font);
                }
                return font;
            }
        }

        public static Font GetNormal(float Size)
        {
            return Get(_normalFamily, Size);
        }

        public static Font GetBold(float Size)
        {
            return Get(_boldFamily, Size, FontStyle.Bold);
        }
    }

    public class FontEntry : IEquatable<FontEntry>
    {
        public readonly FontFamily Family;
        public readonly float Size;
        public readonly FontStyle Style;

        public FontEntry(FontFamily Family, float Size, FontStyle Style)
        {
            this.Family = Family;
            this.Size = Size;
            this.Style = Style;
        }

        public int HashCode => GetHashCode();

        public override int GetHashCode()
        {
            // This Fonts version produces unstable FontFamily hash codes; use its stable metadata.
            return System.HashCode.Combine(Family.Name, Family.Culture.Name, Size, Style);
        }

        public bool Equals(FontEntry other)
        {
            return other != null && Equals(Family, other.Family) && Size.Equals(other.Size) && Style == other.Style;
        }

        public override bool Equals(object obj)
        {
            return obj is FontEntry other && Equals(other);
        }
    }
}
