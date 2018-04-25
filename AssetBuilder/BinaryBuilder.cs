using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBuilder
{
    public class BinaryBuilder : Builder
    {
        public override void Build(string[] Files, string Output)
        {
            var sortedInput = new List<string>(Files);
            sortedInput.Sort(new FileSizeComparer());

            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    for (var i = 0; i < sortedInput.Count; i++)
                    {
                        var data = File.ReadAllBytes(sortedInput[i]);
                        bw.Write(sortedInput[i]);
                        bw.Write(data.Length);
                        bw.Write(data);
                    }
                }
                File.WriteAllBytes(Output, this.ZipBytes(ms.ToArray()) );
            }
        }
    }

    public class FileSizeComparer : IComparer<string>
    {
        public int Compare(string A, string B)
        {
            long s1 = new FileInfo(A).Length;
            long s2 = new FileInfo(B).Length;

            if (s1 == s2) return 0;
            if (s1 > s2) return 1;

            return -1;
        }
    }
}
