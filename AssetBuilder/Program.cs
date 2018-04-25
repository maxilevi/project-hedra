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
        private static readonly Dictionary<string, Builder> Builders;

        static Program()
        {
            Builders = new Dictionary<string, Builder>
            {
                {"binary", new BinaryBuilder()},
                {"text", new TextBuilder()}
            };
        }

        public static void Main(string[] Args)
        {
            if (Args.Length != 3)
            {
                Console.WriteLine("Correct usage: <folder> <file> text|binary");
            }
            var folder = Args[0];
            var file = Args[1];
            var type = Args[2].ToLowerInvariant();
            Console.WriteLine($"Building with sources '{folder}' file '{file}' as '{type}' ");
            Builders[type].Build(Directory.GetFiles(folder, "*", SearchOption.AllDirectories), file);
            Directory.Delete(folder, true);
        }
    }
}
