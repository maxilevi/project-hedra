/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 29/09/2016
 * Time: 03:31 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Hedra.Engine.IO
{
    /// <summary>
    /// Description of Log.
    /// </summary>
    public static class Log
    {
        private static LogType _currentType;
        private static Dictionary<LogType, StreamWriter> _logs;
        private static readonly string LogsPath;
        private static readonly object _lock;

        static Log()
        {
            var log = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/log.txt";
            if(File.Exists(log)) File.Delete(log);
            _lock = new object();
            _logs = new Dictionary<LogType, StreamWriter>();
            /*LogsPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/Logs/";
            Directory.CreateDirectory(LogsPath);
            for (var i = 0; i < (int) LogType.MaxEnums; i++)
            {
                var path = $"{LogsPath}/{((LogType) i).ToString().ToLowerInvariant()}.log";
                if (File.Exists(path)) File.Delete(path);
                _logs.Add((LogType) i, new StreamWriter(new FileStream(path, FileMode.OpenOrCreate))
                {
                    AutoFlush = true
                });
            }*/
            _logs.Add(LogType.Normal, new StreamWriter(new FileStream(log, FileMode.OpenOrCreate))
            {
                AutoFlush = true
            });
        }
        
        public static string Output
        {
            get
            {
                lock (_lock)
                {
                    long position = _logs[_currentType].BaseStream.Position;
                    _logs[_currentType].Close();
                    _logs[_currentType].Dispose();

                    var path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/log.txt";//$"{LogsPath}/{_currentType}.log";
                    string text = File.ReadAllText(path);
                    /*_logs[_currentType] = new StreamWriter(new FileStream(path, FileMode.OpenOrCreate))
                    {
                        AutoFlush = true
                    };*/
                    _logs.Add(LogType.Normal, new StreamWriter(new FileStream(path, FileMode.OpenOrCreate))
                    {
                        AutoFlush = true
                    });
                    _logs[_currentType].BaseStream.Position = position;
                    return text;
                }
            }
        }
        
        public static void WriteToFile(object Text)
        {
            lock (_lock)
            {
                _logs[LogType.Normal].Write(Text.ToString());
            }
        }

        public static void Write(object Text)
        {
            var newText = $"[{DateTime.Now:HH:mm:ss}] {(_currentType != LogType.Normal ? $"[{_currentType.ToString()}]" : string.Empty)} {Text}";
            WriteToFile(newText);
            if(_currentType != LogType.System)
                Console.Write(newText);
            
        }

        public static void Write(object Text, ConsoleColor Color)
        {
            Console.ForegroundColor = Color;
            Write(Text);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void WriteLine(object Line){
            Write( Line + Environment.NewLine);
        }

        public static void WriteLine(string Text, LogType Type)
        {
            var oldType = _currentType;
            _currentType = Type;
            WriteLine(Text);
            _currentType = oldType;
        }

        public static void WriteLine(string Format, params object[] Args)
        {
            for(var i = 0; i < Args.Length; i++)
            {
                Format = Format.Replace("{"+i+"}", Args[i].ToString());
            }
            Write( Format + Environment.NewLine);
        }

        public static void WriteResult(bool Condition, string Text)
        {
            if(Condition)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Log.WriteLine("[SUCCESS] "+Text);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Log.WriteLine("[FAILURE] "+Text);
            }
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void WriteWarning(string Text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Log.WriteLine($"[WARNING] {Text}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void Switch(LogType Type)
        {
            lock (_lock)
            {
                _currentType = Type;
            }
        }

        public static void Flush()
        {
            lock (_lock)
            {
                foreach (var pair in _logs)
                {
                    var stream = pair.Value;
                    stream.Flush();
                }
            }
        }

        public static void FlushAndClose()
        {
            Flush();
            lock (_lock)
            {
                foreach (var pair in _logs)
                {
                    var stream = pair.Value;
                    stream.Close();
                }
            }
        }
    }

    public enum LogType
    {
        Normal,
        IO,
        System,
        WorldBuilding,
        MaxEnums
    }
}
