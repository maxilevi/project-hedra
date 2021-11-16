using System.Linq;
using System.Numerics;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Localization;

namespace Hedra.Mission.Blocks
{
    public class DefendMission : MissionBlock
    {
        private readonly IEntity _entity;
        private readonly IEntity[] _from;

        public DefendMission(IEntity Who, IEntity[] From)
        {
            _entity = Who;
            _from = From;
        }

        public override bool IsCompleted => _from.All(E => E.IsDead);
        public override bool IsFailed => _entity.IsDead;

        public override bool HasLocation => true;
        public override Vector3 Location => _entity.Position;
        public override string ShortDescription => Translations.Get("quest_defend_entity_short", _entity.Name);

        public override string Description => Translations.Get("quest_defend_entity_description", _entity.Name,
            _from.Count(E => !E.IsDead));

        public override DialogObject DefaultOpeningDialog => default;

        public override void Setup()
        {
        }

        public override QuestView BuildView()
        {
            return new EntityView((AnimatedUpdatableModel)_entity.Model);
        }
    }
}