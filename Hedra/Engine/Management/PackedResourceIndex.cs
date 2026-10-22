using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Hedra.Engine.Management
{
    // The archive header is immutable. Parse it once, not on every model/texture read.
    public sealed class PackedResourceIndex
    {
        private readonly Dictionary<string, long> _paths = new(StringComparer.Ordinal);
        private readonly Dictionary<string, List<(string Path, long Offset)>> _fileNames = new(StringComparer.Ordinal);

        public PackedResourceIndex(Stream Stream)
        {
            Stream.Position = 0;
            using var reader = new BinaryReader(Stream, Encoding.UTF8, true);
            while (Stream.Position < Stream.Length)
            {
                var header = reader.ReadString();
                if (header == "<end_header>") break;
                var offset = reader.ReadInt64();
                var path = Normalize(header);
                _paths.TryAdd(path, offset);
                var name = Path.GetFileName(path);
                if (!_fileNames.TryGetValue(name, out var entries))
                    _fileNames.Add(name, entries = new List<(string, long)>());
                entries.Add((path, offset));
            }
        }

        public byte[] Read(Stream Stream, string Name)
        {
            if (!Stream.CanRead) return null;
            var path = Normalize(Name.Replace("$DataFile$", string.Empty).Trim());
            if (!_paths.TryGetValue(path, out var offset))
            {
                if (!_fileNames.TryGetValue(Path.GetFileName(path), out var entries)) return null;
                var found = false;
                // Preserve legacy partial-path matching, including archive order.
                foreach (var entry in entries)
                    if (entry.Path.Contains(path, StringComparison.Ordinal))
                    {
                        offset = entry.Offset;
                        found = true;
                        break;
                    }
                if (!found) return null;
            }
            Stream.Position = offset;
            using var reader = new BinaryReader(Stream, Encoding.UTF8, true);
            return reader.ReadBytes(reader.ReadInt32());
        }

        private static string Normalize(string Path) => Path.Replace('\\', '/');
    }
}
