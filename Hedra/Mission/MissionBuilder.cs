using System.Collections.Generic;
using Hedra.Engine.QuestSystem.Designs;

namespace Hedra.Mission
{
    public class MissionBuilder
    {
        private readonly List<QuestDesign> _designs;
        
        public MissionBuilder()
        {
            _designs = new List<QuestDesign>();
        }
        
        public void Next(QuestDesign Design)
        {
            _designs.Add(Design);
        }
        
        public MissionObject Mission => new MissionObject(_designs.ToArray());
    }
}