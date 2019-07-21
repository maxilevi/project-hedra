using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ScriptMapper
{
    internal class Program
    {
        public static void Main(string[] Args)
        {
            if (Args.Length != 2)
            {
                DisplayHelp();
                return;
            }
            var scripts = Args[0];
            var output = Args[1];
            var processed = ProcessMap(scripts);
            Dump(processed, output);
        }

        private static HashSet<string> ProcessMap(string ScriptsPath)
        {
            var map = new HashSet<string>();
            var scripts = Directory.GetFiles(ScriptsPath).Select(File.ReadAllText).ToArray();
            var blacklist = new List<string>();
            for (var k = 0; k < scripts.Length; ++k)
            {
                scripts[k] = ClearImports(scripts[k], out var words);
                blacklist.AddRange(words);
            }
            var regex = new Regex(
                $@"{
                    string.Join(string.Empty, blacklist.Select(S => $@"(?<!{S}\.)").ToArray())
                   }(?<=\.)([a-zA-Z]*?)[\.\(\s=\[\)]"
            );
            for (var j = 0; j < scripts.Length; j++)
            {
                var matches = regex.Matches(scripts[j]);
                for (var k = 0; k < matches.Count; ++k)
                {
                    var symbol = matches[k].Groups[1].Value;
                    if (!map.Contains(symbol))
                    {
                        map.Add(symbol);
                    }
                }
            } 
            return map;
        }

        private static string ClearImports(string Contents, out string[] ImportedWords)
        {
            var matches = Regex.Matches(Contents, @"import\s+(.*)\s");
            var words = new List<string>();
            for (var i = 0; i < matches.Count; ++i)
            {
                words.AddRange(matches[i].Groups[1].Value.Split(new []
                {
                    ','
                }, StringSplitOptions.RemoveEmptyEntries).Select(S => S.Trim()));
            }
            ImportedWords = new HashSet<string>(words.Where(W => W != "*")).ToArray();
            return Regex.Replace(Contents, @"from\s.+\s|import\s.+\s", string.Empty);
        }

        private static void Dump(HashSet<string> Names, string Path)
        {
            var headers = new[]
            {
                "using System.Reflection;",
                Environment.NewLine
            };
            var lines = Names.OrderBy(N => N).Select(
                N => $"[assembly: Obfuscation(Exclude = false, Feature = \"match-name('^{N}$') and (member-type('field') or member-type('property') or member-type('method') or member-type('event')):-rename\")]"
            ).ToArray();
            File.WriteAllText(Path, string.Join(Environment.NewLine, headers) + string.Join(Environment.NewLine, lines));
        }

        private static void DisplayHelp()
        {
            Console.WriteLine("USAGE: ScriptMapper.exe <path-to-scripts> <path-to-output>");
        }
    }
}