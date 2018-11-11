namespace Hedra.Engine.Localization
{
    public delegate void OnLanguageChanged();
    public class Translation
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
    }
}