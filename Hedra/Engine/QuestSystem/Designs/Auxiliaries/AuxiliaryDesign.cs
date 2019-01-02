using Hedra.Engine.Player.QuestSystem;

namespace Hedra.Engine.QuestSystem.Designs.Auxiliaries
{
    public abstract class AuxiliaryDesign : QuestDesign
    {
        public override QuestTier Tier => QuestTier.Auxiliary;

        public override string Name => null;
        
        public override string ThoughtsKeyword => null;

        public override object[] GetThoughtsParameters(QuestObject Quest)
        {
            return null;
        }

        protected override QuestDesign[] Auxiliaries => null;

        protected override QuestDesign[] Descendants => null;

    }
}