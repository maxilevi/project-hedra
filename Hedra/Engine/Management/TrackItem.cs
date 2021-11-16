using System;

namespace Hedra.Engine.Management
{
    public class TrackItem
    {
        public TrackItem(Func<object> Getter, Action<object> Setter, bool ReleaseFirst)
        {
            this.Getter = Getter;
            this.Setter = Setter;
            this.ReleaseFirst = ReleaseFirst;
        }

        public Func<object> Getter { get; set; }
        public Action<object> Setter { get; set; }
        public bool ReleaseFirst { get; set; }
    }
}