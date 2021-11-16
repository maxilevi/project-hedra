using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedra.Engine.Rendering
{
    public static class CompressedVertexDataExtensions
    {
        public static void Compress<T>(this List<T> Uncompressed, List<CompressedValue<T>> Compressed) where T : struct
        {
            var count = 0;
            var type = default(T);
            var compressing = false;
            Compressed.Clear();
            for (var i = 0; i < Uncompressed.Count; i++)
            {
                if (!compressing)
                {
                    type = Uncompressed[i];
                    count = 1;
                    compressing = true;
                    continue;
                }

                if (type.Equals(Uncompressed[i]))
                {
                    count++;
                }
                else
                {
                    if (count > ushort.MaxValue)
                        throw new ArgumentOutOfRangeException($"Compressed amount exceeds 2^16 ({count})");
                    Compressed.Add(new CompressedValue<T>
                    {
                        Type = type,
                        Count = (ushort)count
                    });
                    type = Uncompressed[i];
                    count = 1;
                }
            }

            if (count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException($"Compressed amount exceeds 2^16 ({count})");
            Compressed.Add(new CompressedValue<T>
            {
                Type = type,
                Count = (ushort)count
            });
        }

        public static List<T> Decompress<T>(this List<CompressedValue<T>> Values) where T : struct
        {
            var list = new List<T>();
            for (var i = 0; i < Values.Count; i++) list.AddRange(Enumerable.Repeat(Values[i].Type, Values[i].Count));
            return list;
        }
    }
}