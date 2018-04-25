using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBuilder
{
    public class TextBuilder : Builder
    {
        public override void Build(string[] Files, string Output)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < Files.Length; i++)
            {
                string fileText = File.ReadAllText(Files[i]);
                builder.AppendLine($"<{Files[i]}>");
                builder.AppendLine(fileText);
                builder.AppendLine("<end>");
            }
            File.WriteAllBytes(Output, this.Zip(builder.ToString()));
        }
    }
}
