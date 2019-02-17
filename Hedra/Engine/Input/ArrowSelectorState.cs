using System;

namespace Hedra.Engine.Input
{
    public class ArrowSelectorState
    {
        public Action OnUp { get; set; }
        public Action OnDown { get; set; }
        public Action OnLeft { get; set; }
        public Action OnRight { get; set; }
        public Action OnEnter { get; set; }
        
        public bool EnterPressed { get; set; }
        public bool LeftPressed { get; set; }
        public bool RightPressed { get; set; }
        public bool UpPressed { get; set; }
        public bool DownPressed { get; set; }
    }
}