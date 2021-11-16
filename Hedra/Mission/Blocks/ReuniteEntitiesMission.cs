using System.Linq;
using System.Numerics;
using Hedra.AISystem;
using Hedra.Core;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.Engine.Rendering;
using Hedra.Engine.Scripting;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Rendering;

namespace Hedra.Mission.Blocks
{
    public class ReuniteEntitiesMission : MissionBlock
    {
        private readonly IEntity[] _entities;
        private readonly Vector3 _position;
        private readonly Timer _timer;

        public ReuniteEntitiesMission(Vector3 Position, IEntity[] Entities)
        {
            _position = Position;
            _entities = Entities;
            _timer = new Timer(0.1f);
        }

        private int NearDistance => FeedableCattleAIComponent.FoodFollowRadius;

        public override bool IsCompleted => EntitiesNotInTarget.Length == 0;

        public override bool HasLocation => true;

        public override Vector3 Location
        {
            get
            {
                var notInTarget = EntitiesNotInTarget;
                for (var i = 0; i < notInTarget.Length; ++i)
                    if ((notInTarget[i].Position - Owner.Position).LengthSquared() > NearDistance * NearDistance)
                        return notInTarget[i].Position;
                return _position;
            }
        }

        public override string ShortDescription => Translations.Get("quest_reunite_entities_short", _entities.Length,
            _entities[0].Name.ToUpperInvariant());

        public override string Description
        {
            get
            {
                if (!IsNearAllEntities)
                    return Translations.Get("quest_reunite_entities_find_description", _entities.Length,
                        _entities[0].Name.ToUpperInvariant());
                return Translations.Get("quest_reunite_entities_goto_description", _entities.Length);
            }
        }

        public override DialogObject DefaultOpeningDialog => default;

        private IEntity[] EntitiesNotInTarget => _entities
            .Where(E => (E.Position - _position).LengthSquared() > NearDistance * NearDistance).ToArray();

        private bool IsNearAllEntities =>
            EntitiesNotInTarget.All(E => (E.Position - Owner.Position).LengthSquared() < NearDistance * NearDistance);

        public override void Setup()
        {
        }

        public override QuestView BuildView()
        {
            return new EntityView((AnimatedUpdatableModel)_entities[0].Model);
        }

        public override void Update()
        {
            base.Update();
            if (!_timer.Tick()) return;
            for (var i = 0; i < _entities.Length; ++i)
                if ((_entities[i].Position - Owner.Position).LengthSquared() > NearDistance * NearDistance)
                {
                    _entities[i].ShowIcon(CacheItem.AttentionIcon);
                    VisualEffects.SetOutline(_entities[i], Colors.DarkRed, true);
                }
                else
                {
                    _entities[i].ShowIcon(null);
                    VisualEffects.SetOutline(_entities[i], Colors.DarkRed, false);
                }
        }
    }
}