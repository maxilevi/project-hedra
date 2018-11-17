using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Hedra.Engine.ItemSystem
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
