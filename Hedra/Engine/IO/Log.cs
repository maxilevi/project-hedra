/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 29/09/2016
 * Time: 03:31 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Reflection;

namespace Hedra.Engine
{
	/// <summary>
	/// Description of Log.
	/// </summary>
	internal static class Log
	{
		private static StreamWriter _writer;
		private static readonly string AppPath;

		static Log(){
			AppPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/";
			if(File.Exists(AppPath+"log.txt")) File.Delete(AppPath+"log.txt");
		    _writer = new StreamWriter(new FileStream(AppPath + "log.txt", FileMode.OpenOrCreate)) {AutoFlush = true};
		}
		
		public static string Output{
			get{ 
				long position = _writer.BaseStream.Position;
				_writer.Close();
				_writer.Dispose();
				
				string text = File.ReadAllText(AppPath+"log.txt");
			    _writer = new StreamWriter(
                    new FileStream(AppPath + "log.txt", FileMode.OpenOrCreate)) {AutoFlush = true};
			    _writer.BaseStream.Position = position;
				return text;
			}
		}
		
		public static void WriteToFile(object Text){
			_writer.Write(Text.ToString());
		}
		public static void Write(object Text){
			Console.Write(Text.ToString());
		    _writer.Write(Text.ToString());
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
		
		public static void WriteLine(string Format, params object[] args){
			for(int i = 0; i < args.Length; i++){
				Format = Format.Replace("{"+i+"}", args[i].ToString());
			}
			Write( Format + Environment.NewLine);
		}

	    public static void WriteResult(bool condition, string text){
			if(condition){
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Log.WriteLine("[SUCCESS] "+text);
			}else{
				Console.ForegroundColor = ConsoleColor.DarkRed;
				Log.WriteLine("[FAILURE] "+text);
			}
			Console.ForegroundColor = ConsoleColor.Gray;
		}
	}
}
