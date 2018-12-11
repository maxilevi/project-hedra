using System.Reflection;

namespace Hedra.Engine.ItemSystem.Templates
{
    [Obfuscation(Exclude = false, Feature = "-rename")]
    public class AttributeTemplate
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public bool Hidden { get; set; }
        public string Display { get; set; }
    }
}
