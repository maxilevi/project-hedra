using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBuilder
{
    public class Program
    {
        private static readonly Dictionary<string, Serializer> Serializers;
        private static readonly Dictionary<string, Builder> Builders;

        static Program()
        {
            Serializers = new Dictionary<string, Serializer>
            {
                {"binary", new BinarySerializer()},
                {"text", new TextSerializer()}
            };
            Builders = new Dictionary<string, Builder>
            {
                {"normal", new NormalBuilder()},
                {"database", new DatabaseBuilder()} 
            };
        }

        public static void Main(string[] Args)
        {
            if (Args.Length != 4)
            {
                Console.WriteLine("Correct usage: <folder> <file> normal|database text|binary");
                return;
            }
            var folder = Args[0];
            var file = Args[1];
            var outputType = Args[2].ToLowerInvariant();
            var type = Args[3].ToLowerInvariant();
            Console.WriteLine($"Building with sources '{folder}' file '{file}' as '{type}' ");
            var output = Serializers[type].Serialize(Directory.GetFiles(folder, "*", SearchOption.AllDirectories));
            Builders[outputType].Build(output, file);
            //Directory.Delete(folder, true);
        }
    }
}
