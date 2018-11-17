using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Hedra.Engine.ItemSystem
{
    [Obfuscation(Exclude = false, Feature = "-rename")]
    public class ItemModelTemplate
    {
        public string Path { get; set; }
        public float Scale { get; set; }
    }
}
