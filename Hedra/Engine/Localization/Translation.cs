using System;

namespace Hedra.Engine.Localization
{
    public delegate void OnLanguageChanged();
    
    public class Translation : IDisposable
    {
        public event OnLanguageChanged LanguageChanged;
        public bool UpperCase { get; set; }
        private string _key;
        private bool _isDefault;
        private string _defaultText;
        private string _format;
        private object[] _parameters;
        private Func<object[], string> _provider;

        private Translation()
        {
            _provider = P => _format.Replace("{0}", _isDefault ? _defaultText : Translations.Get(_key, P));
            Translations.Add(this);
        }
        
        public string Get()
        {
            return UpperCase ? _provider(_parameters).ToUpperInvariant() : _provider(_parameters);
        }

        public void Concat(Func<string> Provider)
        {
            var oldProvider = _provider;
            _provider = P => oldProvider(P) + Provider();
        }
        
        public static Translation Create(string Key, string Format = "{0}", params object[] Parameters)
        {
            return new Translation
            {
                _key = Key,
                _format = Format,
                _parameters = Parameters
            };
        }
        
        public static Translation Create(string Key, params object[] Parameters)
        {
            return Create(Key, "{0}", Parameters);
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

        public void UpdateTranslation()
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