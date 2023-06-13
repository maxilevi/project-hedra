﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.Auth;

class Program
{
        public const string DropboxRefreshToken = "NuiXhqhiMMMAAAAAAAAAAQNs4P9JqJ4UcICvS_4jJsFaB4tbO7LbcSFC84SkExxL";
    
        static async Task Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: program.exe <executable_file_name>");
                return;
            }

            string executableName = args[0];
            string executablePath = Path.Combine(Environment.CurrentDirectory, executableName);

            if (!File.Exists(executablePath))
            {
                Console.WriteLine("Error: {0} does not exist", executableName);
                return;
            }

            Process process = StartProcessWithMiniDump(executablePath);
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                string logFilePath = Path.Combine(Environment.CurrentDirectory, "log.txt");
                string logContents = File.ReadAllText(logFilePath);

                string dumpFilePath = Path.Combine(Environment.CurrentDirectory, executableName + ".dmp");

                string output = GetProcessOutput(process);

                await UploadCrashToDropbox(executableName, logContents, output, dumpFilePath, process.ExitCode);
            }
            else
            {
                Console.WriteLine("Process exited successfully");
            }
        }

        private static Process StartProcessWithMiniDump(string executablePath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(executablePath);
            startInfo.EnvironmentVariables.Add("DOTNET_DbgEnableMiniDump", "1");
            startInfo.EnvironmentVariables.Add("DOTNET_DbgMiniDumpName", Path.Combine(Environment.CurrentDirectory, executablePath + ".dmp"));
            startInfo.EnvironmentVariables.Add("DOTNET_CreateDumpDiagnostics", "1");
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            return Process.Start(startInfo);
        }

        private static string GetProcessOutput(Process process)
        {
            string output = process.StandardOutput.ReadToEnd();
            process.StandardOutput.Close();
            return output;
        }

        private static async Task UploadCrashToDropbox(string executableName, string logContents, string processOutput, string dumpFilePath, int exitCode)
        {
            using (MemoryStream zipStream = new MemoryStream())
            {
                using (ZipArchive zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    ZipArchiveEntry logEntry = zip.CreateEntry("log.txt");
                    using (StreamWriter writer = new StreamWriter(logEntry.Open()))
                    {
                        await writer.WriteAsync(logContents);
                    }

                    ZipArchiveEntry outputEntry = zip.CreateEntry("output.txt");
                    using (StreamWriter writer = new StreamWriter(outputEntry.Open()))
                    {
                        await writer.WriteAsync(processOutput);
                    }

                    if (File.Exists(dumpFilePath))
                    {
                        ZipArchiveEntry dumpEntry = zip.CreateEntry(executableName + ".dmp");
                        using (Stream stream = dumpEntry.Open())
                        {
                            using (FileStream fileStream = new FileStream(dumpFilePath, FileMode.Open))
                            {
                                await fileStream.CopyToAsync(stream);
                            }
                        }
                    }
                }
                
                using (var dbx = new DropboxClient(DropboxRefreshToken, "ylshrx5xujy5bol", "g09mrm6cp7zrhhz"))
                {
                    string uploadPath = "/" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + $"-{exitCode}-crash.zip";
                    zipStream.Seek(0, SeekOrigin.Begin);
                    await dbx.Files.UploadAsync(uploadPath, WriteMode.Overwrite.Instance, body: zipStream);
                    Console.WriteLine("Reported crash");
                }
            }
        }
}