﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;

namespace Hedra.Engine.Testing
{
    public static class Tester
    {
        private static StringBuilder _buffer;

        public static void Run(string LogPath)
        {
            _buffer = new StringBuilder();

            Assembly hedra = Assembly.GetExecutingAssembly();
            foreach (Type type in hedra.GetLoadableTypes())
            {
                if (type.IsSubclassOf(typeof(BaseTest)) && type != typeof(BaseTest))
                {
                    var test = (BaseTest) Activator.CreateInstance(type);
                    test.Run();
                }
            }
            Tester.Dump(LogPath);
        }

        public static void Log(string Text)
        {
            _buffer.Append(Text);
        }

        private static void Dump(string Path)
        {
            if (File.Exists(Path)) File.Delete(Path);
            File.WriteAllText(Path, _buffer.ToString());
            _buffer.Clear();
        }
    }
}