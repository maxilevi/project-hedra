using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Hedra.Engine.IO;

namespace Hedra.Engine.Rendering.UI
{
    public static class FontCache
    {
        private static readonly Dictionary<FontEntry, Font> CachedFonts = new Dictionary<FontEntry, Font>();

        public static Font Get(FontFamily Family, float Size, FontStyle Style = FontStyle.Regular)
        {
            var entry = new FontEntry(Family, Size, Style);
            if (!CachedFonts.ContainsKey(entry))
            {
                try
                {
                    CachedFonts.Add(entry, new Font(entry.Family, entry.Size, entry.Style));
                }
                catch (ArgumentException e)
                {
                    Log.WriteLine($"Font '{Family.Name}' with size '{Size}' ans style '{entry.Style}' failed to create.");
                    if(CachedFonts.Count > 0)
                        return CachedFonts.Values.First();
                    throw;
                }
            }
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
