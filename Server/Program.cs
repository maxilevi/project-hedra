using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Server
{
    public class Program
    {
        public static void Main(string[] Args)
        {
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Hedra.exe",
                Arguments = "--server-mode"
            };
            Process.Start(startInfo);
            Environment.Exit(0);
        }
    }
}