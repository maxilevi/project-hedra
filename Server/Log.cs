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

namespace Server
{
    /// <summary>
    /// Description of Log.
    /// </summary>
    public static class Log
    {
        private static StreamWriter Writer;
        private static string AppPath;
        static Log(){
            AppPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/";
            if(File.Exists(AppPath+"log.txt")) File.Delete(AppPath+"log.txt");
            Writer = new StreamWriter(new FileStream(AppPath+"log.txt", FileMode.OpenOrCreate));
            Writer.AutoFlush = true;
        }

        public static string Output{
            get{
                long Position = Writer.BaseStream.Position;
                Writer.Close();
                Writer.Dispose();

                string Text = File.ReadAllText(AppPath+"log.txt");
                Writer = new StreamWriter(new FileStream(AppPath+"log.txt", FileMode.OpenOrCreate));
                Writer.AutoFlush = true;
                Writer.BaseStream.Position = Position;

                return Text;
            }
        }

        public static void Clear()
        {
            Console.Clear();
            Writer.Close();
            Writer.Dispose();

            if(File.Exists(AppPath+"log.txt")) File.Delete(AppPath+"log.txt");

            Writer = new StreamWriter(new FileStream(AppPath+"log.txt", FileMode.OpenOrCreate));
            Writer.AutoFlush = true;
        }

        public static void WriteToFile(object Text){
            Writer.Write(Text.ToString());
        }
        public static void Write(object Text){
            Writer.Write(Text.ToString());
            Console.Write(Text.ToString());
        }

        public static void WriteLine(object Line){
            Write( Line.ToString() + Environment.NewLine);
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
