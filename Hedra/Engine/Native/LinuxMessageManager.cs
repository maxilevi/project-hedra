using System;
using System.Diagnostics;

namespace Hedra.Engine.Native
{
    public class LinuxMessageManager : IMessageManager
    {
        public void Show(string Message, string Title)
        {
            var proc = new Process
            {
                StartInfo =
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"zenity --error --text='{Message}' --title='{Title}'\"",
                    UseShellExecute = false
                }
            };
            proc.Start();
            Console.WriteLine($"{Title}: {Message}");
        }
    }
}