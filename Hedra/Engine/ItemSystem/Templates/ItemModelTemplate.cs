using System.Reflection;

namespace Hedra.Engine.ItemSystem.Templates
{
    [Obfuscation(Exclude = false, Feature = "-rename")]
    public class ItemModelTemplate
    {
        public string Path { get; set; }
        public float Scale { get; set; }
    }
}
