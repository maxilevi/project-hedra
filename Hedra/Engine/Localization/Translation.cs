namespace Hedra.Engine.Localization
{
    public delegate void OnLanguageChanged();
    public class Translation
    {
        public event OnLanguageChanged LanguageChanged;
        public string Key { get; set; }
        private bool _isDefault;
        private string _defaultText;

        public string Get()
        {
            return _isDefault ? _defaultText : Translations.Get(Key);
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