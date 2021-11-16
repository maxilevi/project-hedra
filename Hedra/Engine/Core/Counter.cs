namespace Hedra.Engine.Core
{
    public class Counter
    {
        private int _ticks;

        public Counter(int Ticks)
        {
            TickCount = Ticks;
        }

        public int TickCount { get; set; }

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