namespace Hedra.Engine.Core
{
    public class Counter
    {
        public int TickCount { get; set; }
        private int _ticks;
        
        public Counter(int Ticks)
        {
            TickCount = Ticks;
        }

        public bool Tick()
        {
            if (_ticks == TickCount)
                return true;
            _ticks++;
            return false;
        }

        public void Reset()
        {
            _ticks = 0;
        }
    }
}