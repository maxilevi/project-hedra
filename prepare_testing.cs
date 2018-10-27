using System;
using System.IO;
using System.Text.RegularExpressions;

public static class TestingMode
{
    public static void Main(string[] Args)
    {
        const string projectPath = "./Hedra/Hedra.csproj";
        var currentProj = File.ReadAllText(projectPath);
        currentProj = Regex.Replace(currentProj,
            "<None.*?<\\/None>",
            string.Empty,
            RegexOptions.Singleline
        );
        currentProj = Regex.Replace(currentProj,
            "<Content.*?<\\/Content>",
            string.Empty,
            RegexOptions.Singleline
        );
        currentProj = Regex.Replace(currentProj,
            "<PostBuildEvent.*?<\\/PostBuildEvent>",
            string.Empty,
            RegexOptions.Singleline
        );
        var outPath = $"{projectPath}.test";
        File.WriteAllText(outPath, currentProj);
        Console.WriteLine($"Generated testing '.csproj' at '{outPath}'");
    }
}