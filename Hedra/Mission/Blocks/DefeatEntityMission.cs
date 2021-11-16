using System.Linq;
using Hedra.EntitySystem;
using Hedra.Localization;

namespace Hedra.Mission.Blocks
{
    public class DefeatEntityMission : EntityMission
    {
        public DefeatEntityMission(params IEntity[] Entities) : base(Entities)
        {
        }

        public override bool IsCompleted => Entities.All(E => E.IsDead);

        public override string ShortDescription =>
            Entities.Length == 1
                ? Translations.Get("quest_defeat_entity_short", Entity.Name)
                : Translations.Get("quest_defeat_entities_short", Entities.Length, Entity.Name);

        public override string Description =>
            Entities.Length == 1
                ? Translations.Get("quest_defeat_entity_description", Entity.Name)
                : Translations.Get("quest_defeat_entities_description", Entities.Count(E => !E.IsDead), Entity.Name);

        public override DialogObject DefaultOpeningDialog => default;
    }
}