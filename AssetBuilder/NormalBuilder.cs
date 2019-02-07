using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetBuilder
{
    public class NormalBuilder : Builder
    {
        public override void Build(Dictionary<string, object> Input, string Output)
        {
            switch (Input.Values.First())
            {
                case string _:
                    this.TextBuild(Input, Output);
                    break;

                case byte[] _:
                    this.BinaryBuild(Input, Output);
                    break;
            }
        }

        private void TextBuild(Dictionary<string, object> Input, string Output)
        {
            var builder = new StringBuilder();
            foreach (var pair in Input)
            {
                string fileText = (string) pair.Value;
                builder.AppendLine($"<{pair.Key}>");
                builder.AppendLine(fileText);
                builder.AppendLine("<end>");
            }
            File.WriteAllBytes(Output, this.Zip(builder.ToString()));
        }

        private void BinaryBuild(Dictionary<string, object> Input, string Output)
        {
            var map = new Dictionary<string, long>();
            byte[] contents;
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    foreach (var pair in Input)
                    {
                        var data = (byte[]) pair.Value;
                        map.Add(pair.Key, bw.BaseStream.Position);
                        bw.Write(data.Length);
                        bw.Write(data);
                    }
                }
                contents = ms.ToArray();
            }

            byte[] index;
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    BuildIndex(bw, map, 0);
                }
                index = ms.ToArray();
            }

            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms, Encoding.ASCII))
                {

                    var separatorLength = Encoding.ASCII.GetByteCount("<end_header>") + 1;
                    BuildIndex(bw, map, index.Length + separatorLength);
                    bw.Write("<end_header>");
                    bw.Write(contents);
                }
                File.WriteAllBytes(Output, ZipBytes(ms.ToArray()));
            }
        }

        private static void BuildIndex(BinaryWriter Bw, Dictionary<string, long> Input, int Offset)
        {
            foreach (var pair in Input)
            {
                Bw.Write(pair.Key);
                Bw.Write((long)(pair.Value + Offset));
            }
        }
    }
}
