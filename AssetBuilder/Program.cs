using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace AssetBuilder
{
    public class Program
    {
        private static readonly Dictionary<string, Serializer> Serializers;
        private static readonly Dictionary<string, Builder> Builders;
        private static readonly string AppPath;

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
            AppPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/";
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
            var projectName = new DirectoryInfo(folder).Name;
            Console.WriteLine($" folder is '{folder}' name is '{projectName}' ");
            var projectFiles = Directory.GetFiles(folder, "*", SearchOption.AllDirectories).OrderBy(P => P).ToArray();
            var history = LoadHistory(projectName);
            var needsRebuild = history == null || ShouldWeRebuild(history.Builds, projectFiles);
            if (needsRebuild || !File.Exists(file))
            {
                Console.WriteLine($"{(history == null ? "Build history is null" : needsRebuild ? "Build history is obsolete" : "File doesnt exist, ")}, rebuilding...");
                var output = Serializers[type].Serialize(projectFiles);
                Builders[outputType].Build(output.Results, file);
                output.History.Builds = output.History.Builds.OrderBy(B => B.Path).ToArray();
                WriteHistory(projectName, output.History);
            }
            else
            {
                Console.WriteLine($"Build history was found, skipping rebuild.");
            }
        }

        private static void WriteHistory(string Name, BuildHistory History)
        {
            var historyPath = $"{AppPath}/{Name}.hst";
            File.WriteAllText(historyPath, BuildHistory.To(History));
        }

        private static BuildHistory LoadHistory(string Name)
        {
            var historyPath = $"{AppPath}/{Name}.hst";
            if (!File.Exists(historyPath)) return null;
            return BuildHistory.From(File.ReadAllLines(historyPath));
        }

        private static bool ShouldWeRebuild(AssetBuild[] History, string[] Files)
        {
            if (Files.Length != History.Length) return true;
            for (var i = 0; i < Files.Length; i++)
            {
                if (Files[i] != History[i].Path || ChecksumDiffers(Files[i], History[i].Checksum))
                {
                    if (Files[i] != History[i].Path)
                    {
                        Console.WriteLine($"Found history file mismatch at {Files[i]} vs {History[i].Path}");
                    }
                    else
                    {
                        Console.WriteLine($"Found history checksum mismatch at {AssetBuild.CreateHash(Files[i])} vs {History[i].Checksum}");
                    }
                    return true;
                }
            }
            return false;
        }

        private static bool ChecksumDiffers(string Path, string Checksum)
        {
            return AssetBuild.CreateHash(Path) != Checksum;
        }
    }
}
