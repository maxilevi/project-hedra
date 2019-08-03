using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.StructureSystem;
using OpenTK;

namespace Hedra.Mission.Blocks
{
    public class CompleteStructureMission : MissionBlock
    {
        public ICompletableStructure StructureObject { get; set; }
        public ICompletableStructureDesign StructureDesign { get; set; }
        public override bool IsCompleted => StructureObject.Completed;
        public override void Setup()
        {
            throw new System.NotImplementedException();
        }

        public override QuestView BuildView()
        {
            return new ModelView(StructureDesign.Icon);
        }

        public override bool HasLocation => true;
        public override Vector3 Location => StructureObject.Position;
        public override string ShortDescription => StructureDesign.GetShortDescription(StructureObject);
        public override string Description => StructureDesign.GetDescription(StructureObject);
        public override DialogObject DefaultOpeningDialog => null;
    }
}