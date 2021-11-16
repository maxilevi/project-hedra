using Hedra.EntitySystem;
using Hedra.Localization;

namespace Hedra.Mission.Blocks
{
    public class FindEntityMission : EntityMission
    {
        public FindEntityMission(IEntity Entity) : base(Entity)
        {
        }

        public override bool IsCompleted => (Entity.Position - Owner.Position).LengthSquared() < 48 * 48;
        public override string ShortDescription => Translations.Get("quest_find_entity_short", Entity.Name);
        public override string Description => Translations.Get("quest_find_entity_description", Entity.Name);
        public override DialogObject DefaultOpeningDialog => default;
    }
}