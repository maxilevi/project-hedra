using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

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
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
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
            var projectInfo = new DirectoryInfo(folder);
            var projectName = projectInfo.Name;
            var parentName = UniqueFrom(folder);
            Console.WriteLine($" folder is '{folder}' name is '{projectName}' ");
            var projectFiles = Directory.GetFiles(folder, "*", SearchOption.AllDirectories).OrderBy(P => P).ToArray();
            var history = LoadHistory(projectName, parentName);
            var needsRebuild = history == null || ShouldWeRebuild(history.Builds, projectFiles);
            if (needsRebuild || !File.Exists(file))
            {
                Console.WriteLine($"{(history == null ? "Build history is null" : needsRebuild ? "Build history is obsolete" : "File doesnt exist, ")}, rebuilding...");
                var output = Serializers[type].Serialize(projectFiles);
                Builders[outputType].Build(output.Results, file);
                output.History.Builds = output.History.Builds.OrderBy(B => B.Path).ToArray();
                WriteHistory(projectName, parentName, output.History);
            }
            else
            {
                Console.WriteLine($"Build history was found, skipping rebuild.");
            }
        }

        private static void WriteHistory(string Name, string ParentName, BuildHistory History)
        {
            var historyPath = $"{AppPath}/{ParentName}/{Name}.hst";
            Console.WriteLine(historyPath);
            Directory.CreateDirectory($"{AppPath}/{ParentName}/");
            File.WriteAllText(historyPath, BuildHistory.To(History));
        }

        private static BuildHistory LoadHistory(string Name, string ParentName)
        {
            var historyPath = $"{AppPath}/{ParentName}/{Name}.hst";
            Console.WriteLine(historyPath);
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

        private static string UniqueFrom(string Path)
        {
            using var md5Hasher = MD5.Create();
            var hashed = md5Hasher.ComputeHash(Encoding.ASCII.GetBytes(Path));
            return System.Convert.ToBase64String(hashed).Replace("+", "-").Replace("/", "_");
        }
    }
}
