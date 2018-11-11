using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Hedra.Engine.Game;

namespace Hedra.Engine.Localization
{
    public static class Translations
    {
        private static readonly Dictionary<string, Dictionary<string, string>> _translations;
        public static string Language { get; set; } = GameLanguage.English.ToString();

        static Translations()
        {
            _translations = new Dictionary<string, Dictionary<string, string>>();
        }
        
        public static void Load()
        {
            _translations.Clear();
            var files = Directory.GetFiles($"{GameLoader.AppPath}/Translations/");
            for (var i = 0; i < files.Length; i++)
            {
                _translations.Add(
                    Path.GetFileNameWithoutExtension(files[i]),
                    Parse(File.ReadAllText(files[i]))
                );
            }
        }
               
        public static string Get(string Key)
        {
            return Get(Key, Language);
        }

        private static string Get(string Key, GameLanguage AppLanguage)
        {
            return Get(Key, AppLanguage.ToString());
        }

        private static string Get(string Key, string AppLanguage)
        {
            string Fail()
            {
                if (AppLanguage == GameLanguage.English.ToString())
                    throw new ArgumentException("Failed to get translations in the default language (english)");
                return Get(Key, GameLanguage.English); 
            }
            
            if (!_translations.ContainsKey(AppLanguage)) return Fail();
            if (!_translations[AppLanguage].ContainsKey(Key)) return Fail();

            return _translations[AppLanguage][Key];
        }

        private static Dictionary<string, string> Parse(string Contents)
        {
            var dict = new Dictionary<string, string>();
            var lines = Contents.Split(Environment.NewLine.ToCharArray()).Where(S => !string.IsNullOrEmpty(S)).ToArray();
            for (var i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split('=');
                var key = parts[0].Trim();
                var val = parts[1].Trim();
                dict.Add(key, val);
            }
            return dict;
        }

        public static string[] Languages => _translations.Keys.ToArray();
    }
}
