﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hedra.Engine.Game;
using Hedra.Engine.Localization;

namespace Hedra.Localization
{
    public static class Translations
    {
        private static readonly Dictionary<string, Dictionary<string, string>> _translations;
        private static readonly List<Translation> _liveTranslations;

        static Translations()
        {
            _translations = new Dictionary<string, Dictionary<string, string>>();
            _liveTranslations = new List<Translation>();
        }
        
        public static void Load()
        {
            _translations.Clear();
            var files = Directory.GetFiles($"{GameLoader.AppPath}/Translations/");
            for (var i = 0; i < files.Length; i++)
            {
                _translations.Add(
                    Path.GetFileNameWithoutExtension(files[i]),
                    IniParser.Parse(File.ReadAllText(files[i]))
                );
            }
        }

        public static void Add(Translation Key)
        {
            _liveTranslations.Add(Key);
        }
        
        public static void Remove(Translation Key)
        {
            _liveTranslations.Remove(Key);
        }
 
        public static string Get(string Key)
        {
            return Get(Key, new object[0], Language);
        }
        
        public static string Get(string Key, params object[] Params)
        {
            return Get(Key, Params, Language);
        }

        private static string Get(string Key, object[] Params, GameLanguage AppLanguage)
        {
            return Get(Key, Params, AppLanguage.ToString());
        }

        private static string Get(string Key,  object[] Params, string AppLanguage)
        {
            string Fail()
            {
                if (AppLanguage == GameLanguage.English.ToString())
                    throw new ArgumentException($"Failed to get key '{Key}' in the default language (english)");
                return Get(Key, Params, GameLanguage.English); 
            }
            
            if (!_translations.ContainsKey(AppLanguage)) return Fail();
            if (!_translations[AppLanguage].ContainsKey(Key)) return Fail();

            return AddParameters(_translations[AppLanguage][Key], Params);
        }
        
        public static bool Has(string Key)
        {
            return Has(Key, Language);
        }
        
        private static bool Has(string Key, GameLanguage AppLanguage)
        {
            return Has(Key, AppLanguage.ToString());
        }
        
        private static bool Has(string Key, string AppLanguage)
        {
            if (_translations.ContainsKey(AppLanguage) && _translations[AppLanguage].ContainsKey(Key)) return true;
            if (AppLanguage == GameLanguage.English.ToString()) return false;
            return Has(Key, GameLanguage.English);
        }

        private static string AddParameters(string Value, object[] Params)
        {
            for (var i = 0; i < Params.Length; i++)
            {
                Value = Value.Replace("{" + i + "}", Params[i].ToString());
            }
            return Value.Replace(@"\n", Environment.NewLine);
        }
        
        public static string[] Languages => _translations.Keys.ToArray();
        
        private static string _language = GameLanguage.English.ToString();

        public static string Language
        {
            get => _language;
            set
            {
                _language = value;
                for (var i = 0; i < _liveTranslations.Count; i++)
                {
                    _liveTranslations[i].UpdateTranslation();
                }
            }
        }
    }
}