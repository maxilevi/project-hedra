using Hedra.Mission.Blocks;

namespace Hedra.Mission
{
    public class MissionObject
    {
        private readonly MissionBlock[] _blocks;
        
        public MissionObject(MissionBlock[] Blocks)
        {
            _blocks = Blocks;
        }

        public MissionObject Clone()
        {
            return new MissionObject(_blocks);
        }
    }
}