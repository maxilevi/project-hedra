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

        private Translation()
        {
        }
        
        public string Get()
        {
            return _isDefault ? _defaultText : Translations.Get(_key);
        }

        public static Translation Create(string Key)
        {
            return new Translation
            {
                _key = Key
            };
        }

        public static Translation Default(string Text)
        {
            return new Translation
            {
                _isDefault = true,
                _defaultText = Text
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
        }
    }
}