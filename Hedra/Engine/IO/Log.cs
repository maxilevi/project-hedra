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

namespace Hedra.Engine
{
	/// <summary>
	/// Description of Log.
	/// </summary>
	internal static class Log
	{
	    private static LogType _currentType;
	    private static Dictionary<LogType, StreamWriter> _logs;
		private static readonly string LogsPath;
	    private static readonly object _lock;

		static Log()
        {
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
            _logs.Add(LogType.Normal, new StreamWriter(new FileStream($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/log.txt", FileMode.OpenOrCreate))
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
            //lock (_lock)
            {
                _logs[_currentType].Write(Text.ToString());
            }
        }

		public static void Write(object Text)
		{
		    var newText = $"[{DateTime.Now:HH:mm:ss}] {Text}";
            if(_currentType == LogType.Normal) Console.Write(newText);
            WriteToFile(newText);
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
            RunWithType(Type, () => WriteLine(Text));
	    }

	    public static void WriteLine(string Format, params object[] Args)
        {
			for(int i = 0; i < Args.Length; i++)
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

	    public static void Switch(LogType Type)
	    {
	        //lock (_lock)
	        {
	            _currentType = Type;
	        }
	    }

	    public static void RunWithType(LogType Type, Action Job)
	    {
	        //lock (_lock)
	        {
	            //var oldType = _currentType;
	            //_currentType = Type;
	            Job();
	            //_currentType = oldType;
	        }
	    }
	}

    internal enum LogType
    {
        Normal,
        IO,
        WorldBuilding,
        MaxEnums
    }
}
