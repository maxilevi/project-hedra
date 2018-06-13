using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBuilder
{
    public class TextSerializer : Serializer
    {
        public override Dictionary<string, object> Serialize(string[] Files)
        {
            var output = new Dictionary<string, object>();
            for (var i = 0; i < Files.Length; i++)
            {
                string fileText = File.ReadAllText(Files[i]);
                output.Add(Files[i], fileText);
            }
            return output;
        }
    }
}
