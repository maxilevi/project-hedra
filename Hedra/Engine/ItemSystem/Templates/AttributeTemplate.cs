namespace Hedra.Engine.ItemSystem.Templates
{
    public class AttributeTemplate
    {
        private bool _persist;
        public string Name { get; set; }
        public object Value { get; set; }
        public bool Hidden { get; set; }
        public string Display { get; set; }

        public bool Persist
        {
            get => _persist || Name == CommonAttributes.Amount.ToString();
            set => _persist = value;
        }
    }
}