using System;
using Hedra.BiomeSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.StructureSystem;
using Hedra.Game;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.QuestSystem
{
    public class QuestStructureDesign : StructureDesign
    {
        private bool _canRemove;
        private readonly QuestObject _quest;
        
        public QuestStructureDesign(QuestObject Quest)
        {
            _quest = Quest;
            _canRemove = false;
            GameManager.Player.Questing.QuestAbandoned += OnQuestChanged;
            GameManager.Player.Questing.QuestCompleted += OnQuestChanged;
        }

        private void OnQuestChanged(QuestObject Quest)
        {
            if(Quest == _quest)
                _canRemove = true;
        }
        
        public override int PlateauRadius
            => throw new NotImplementedException();
        
        public override VertexData Icon => null;
        
        public override void Build(CollidableStructure Structure)
            => throw new NotImplementedException();

        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
            => throw new NotImplementedException();

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, IRandom Rng)
            => throw new NotImplementedException();
        
        public override bool ShouldRemove(CollidableStructure Structure)
        {
            return World.GetChunkByOffset(World.ToChunkSpace(Structure.Position)) == null && _canRemove;
        }
    }
}