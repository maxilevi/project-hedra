using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hedra.Engine.Management
{
    public class TrackItem
    {
        public Func<object> Getter { get; set; }
        public Action<object> Setter { get; set; }

        public TrackItem(Func<object> Getter, Action<object> Setter)
        {
            this.Getter = Getter;
            this.Setter = Setter;
        }
    }
}
