using Hedra.Engine.Player.QuestSystem;

namespace Hedra.Engine.QuestSystem.Designs.Auxiliaries
{
    public abstract class AuxiliaryDesign : QuestDesign
    {
        protected override bool IsAuxiliary => false;
        
        public override QuestTier Tier => QuestTier.Auxiliary;
        
        public override QuestView View => new DefaultView();
        
        public override QuestDesign[] Auxiliaries => null;

        public override QuestDesign[] Descendants => null;

    }
}