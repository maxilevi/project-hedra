using System;

namespace Hedra.Engine.Localization
{
    public delegate void OnLanguageChanged();
    
    public class Translation : IDisposable
    {
        public event OnLanguageChanged LanguageChanged;
        private string _key;
        private bool _isDefault;
        private string _defaultText;
        private string _format;

        private Translation()
        {
        }
        
        public string Get()
        {
            return _format.Replace("{0}", _isDefault ? _defaultText : Translations.Get(_key));
        }

        public static Translation Create(string Key, string Format = "{0}")
        {
            return new Translation
            {
                _key = Key,
                _format = Format
            };
        }

        public static Translation Default(string Text)
        {
            return new Translation
            {
                _isDefault = true,
                _defaultText = Text,
                _format = "{0}"
            };
        }

        public void UpdateTranslation(string NewLanguage)
        {
            LanguageChanged?.Invoke();
        }

        public void Dispose()
        {
            if (LanguageChanged != null)
            {
                var list = LanguageChanged.GetInvocationList();
                for(var i = 0; i < list.Length; i++)
                {
                    LanguageChanged -= (OnLanguageChanged) list[i];
                }
            }
            Translations.Remove(this);
        }
    }
}