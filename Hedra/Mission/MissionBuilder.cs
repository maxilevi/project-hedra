using System.Collections.Generic;
using Hedra.Mission.Blocks;

namespace Hedra.Mission
{
    public class MissionBuilder
    {
        private readonly List<MissionBlock> _designs;
        
        public MissionBuilder()
        {
            _designs = new List<MissionBlock>();
        }
        
        public void Next(MissionBlock Design)
        {
            _designs.Add(Design);
        }
        
        public MissionObject Mission => new MissionObject(_designs.ToArray());
    }
}