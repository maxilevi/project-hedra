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
        public override SerializationOutput Serialize(string[] Files)
        {
            var output = new Dictionary<string, object>();
            var builds = new List<AssetBuild>();
            for (var i = 0; i < Files.Length; i++)
            {
                string fileText = File.ReadAllText(Files[i]);
                output.Add(Files[i], fileText);
                builds.Add(new AssetBuild
                {
                    Path = Files[i],
                    Checksum = AssetBuild.CreateHash(Files[i])
                });
            }
            return new SerializationOutput
            {
                Results = output,
                History = new BuildHistory
                {
                    Builds = builds.ToArray()
                }
            };
        }
    }
}
