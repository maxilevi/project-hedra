using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using Hedra.Engine.IO;
using SixLabors.Fonts;

namespace Hedra.Rendering.UI
{
    public static class FontCache
    {
        private static readonly Dictionary<FontEntry, Font> CachedFonts = new Dictionary<FontEntry, Font>();
        private static FontFamily _normalFamily = new FontFamily(GenericFontFamilies.Serif);
        private static FontFamily _boldFamily = new FontFamily(GenericFontFamilies.Serif);

        public static void SetFonts(FontFamily NormalCollection, FontFamily BoldCollection)
        {
            _normalFamily = NormalCollection;
            _boldFamily = BoldCollection;
        }

        public static Font Get(Font Original, float Size)
        {
            return Get(Original.Family, Size, Original.Style);
        }

        private static Font Get(FontFamily Family, float Size, FontStyle Style = FontStyle.Regular)
        {
            var entry = new FontEntry(Family, Size, Style);
            if (!CachedFonts.ContainsKey(entry))
                try
                {
                    CachedFonts.Add(entry, new Font(entry.Family, entry.Size, entry.Style));
                }
                catch (ArgumentException e)
                {
                    Log.WriteLine(Family == null
                        ? $"Provided font family was null.{Environment.NewLine}Trace:{e}"
                        : $"Font '{Family.Name}' with size '{Size}' and style '{entry.Style}' failed to create.{Environment.NewLine}Trace:{e}");
                    if (CachedFonts.Count > 0)
                        return CachedFonts.Values.First();
                    throw;
                }

            return CachedFonts[entry];
        }

        public static Font Default => Get(_normalFamily, 10);

        public static Font GetNormal(float Size)
        {
            return Get(_normalFamily, Size);
        }

        public static Font GetBold(float Size)
        {
            return Get(_boldFamily, Size, FontStyle.Bold);
        }

        public static int Count => CachedFonts.Count;
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

        public int HashCode => GetHashCode();

        public override int GetHashCode()
        {
            return Family.GetHashCode() + (int)Size * 29 + (int)Style * 13;
        }

        public override bool Equals(object obj)
        {
            return obj is FontEntry other && other.GetHashCode() == GetHashCode();
        }
    }
}