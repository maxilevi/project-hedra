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
    ///     Description of Log.
    /// </summary>
    public static class Log
    {
        private static readonly string LogsPath;
        private static readonly object _lock;
        private static StreamWriter _writer;

        static Log()
        {
            var log = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/log.txt";
            if (File.Exists(log)) File.Delete(log);
            _lock = new object();
            _writer = new StreamWriter(new FileStream(log, FileMode.OpenOrCreate));
            //Console.SetOut(_writer);
        }
        
        public static void WriteToFile(object Text)
        {
            lock (_lock)
            {
                _writer.Write(Text.ToString());
            }
        }

        public static void Write(object Text)
        {
            var newText =
                $"[{DateTime.Now:HH:mm:ss}] {Text}";
            WriteToFile(newText);
            //Console.Write(newText);
        }

        public static void Write(object Text, ConsoleColor Color)
        {
            //Console.ForegroundColor = Color;
            Write(Text);
            //Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void WriteLine(object Line)
        {
            Write(Line + Environment.NewLine);
        }

        public static void WriteLine(string Text, LogType Type)
        {
            //var oldType = _currentType;
            //_currentType = Type;
            WriteLine(Text);
            //_currentType = oldType;
        }

        public static void WriteLine(string Format, params object[] Args)
        {
            for (var i = 0; i < Args.Length; i++) Format = Format.Replace("{" + i + "}", Args[i].ToString());
            Write(Format + Environment.NewLine);
        }

        public static void WriteResult(bool Condition, string Text)
        {
            if (Condition)
            {
                //Console.ForegroundColor = ConsoleColor.DarkGreen;
                WriteLine("[SUCCESS] " + Text);
            }
            else
            {
                //Console.ForegroundColor = ConsoleColor.DarkRed;
                WriteLine("[FAILURE] " + Text);
            }

            //Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void WriteWarning(string Text)
        {
            //Console.ForegroundColor = ConsoleColor.Yellow;
            WriteLine($"[WARNING] {Text}");
            //Console.ForegroundColor = ConsoleColor.Gray;
        }
        
        public static void FlushAndClose()
        {
            lock (_lock)
            {
                _writer.Flush();
                _writer.Close();
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