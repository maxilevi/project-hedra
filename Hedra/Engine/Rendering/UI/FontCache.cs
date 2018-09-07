using System;
using System.Collections.Generic;
using System.Drawing;

namespace Hedra.Engine.Rendering.UI
{
    public static class FontCache
    {
        private static readonly Dictionary<FontEntry, Font> CachedFonts = new Dictionary<FontEntry, Font>();

        public static Font Get(FontFamily Family, float Size, FontStyle Style = FontStyle.Regular)
        {
            //if (!Family.IsStyleAvailable(Style))
            //    return CachedFonts.Values.FirstOrDefault();

            var entry = new FontEntry(Family, Size, Style);
            if (!CachedFonts.ContainsKey(entry))
                CachedFonts.Add( entry, new Font(entry.Family, entry.Size, entry.Style) );
            
            return CachedFonts[entry];
        }
    }

    public class FontEntry
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

        public int HashCode => this.GetHashCode();

        public override int GetHashCode()
        {
            return Family.GetHashCode() + (int) Size * 29 + (int) Style * 13;
        }

        public override bool Equals(object obj)
        {
            return obj is FontEntry other && other.GetHashCode() == this.GetHashCode();
        }
    }
}
