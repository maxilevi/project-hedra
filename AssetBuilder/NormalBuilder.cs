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
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    foreach (var pair in Input)
                    {
                        var data = (byte[]) pair.Value;
                        bw.Write(pair.Key);
                        bw.Write(data.Length);
                        bw.Write(data);
                    }
                }
                File.WriteAllBytes(Output, this.ZipBytes(ms.ToArray()));
            }
        }
    }
}
