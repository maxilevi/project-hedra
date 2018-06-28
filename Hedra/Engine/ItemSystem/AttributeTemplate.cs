using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hedra.Engine.ItemSystem
{
    internal class AttributeTemplate
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public bool Hidden { get; set; }
        public string Display { get; set; }
    }
}
