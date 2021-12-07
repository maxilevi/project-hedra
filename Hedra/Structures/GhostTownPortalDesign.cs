using System;
using System.Numerics;
using Hedra.Components;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.GhostTown;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Rendering;

namespace Hedra.Structures
{
    public class GhostTownPortalDesign : SimpleFindableStructureDesign<GhostTownPortal>, ICompletableStructureDesign
    {
        public override int PlateauRadius => 140;
        public override int StructureChance => StructureGrid.GhostTownPortalChance;
        protected override CacheItem? Cache => CacheItem.Portal;
        protected override Vector3 StructureScale => Vector3.One * 10;
        protected virtual bool SpawnNPC => true;
        public override bool CanSpawnInside => true;

        public override string DisplayName => Translations.Get("structure_portal");
        public override VertexData Icon { get; } = CacheManager.GetModel(CacheItem.PortalIcon);

        public string GetShortDescription(IStructure Structure)
        {
            return Translations.Get("quest_complete_portal_short");
        }

        public string GetDescription(IStructure Structure)
        {
            return Translations.Get("quest_complete_portal_description");
        }

        protected override void DoBuild(CollidableStructure Structure, Matrix4x4 Rotation, Matrix4x4 Translation,
            Random Rng)
        {
            base.DoBuild(Structure, Rotation, Translation, Rng);
            if (!SpawnNPC) return;
            DoWhenChunkReady(
                Vector3.Transform(-Vector3.UnitZ * 5 * StructureScale - Vector3.UnitX * 2 * StructureScale,
                    Rotation * Translation), P =>
                {
                    var human = NPCCreator.SpawnHumanoid(HumanType.Mage, P);
                    human.Physics.UsePhysics = false;
                    human.Position = P;
                    human.SearchComponent<DamageComponent>().Immune = true;
                    human.RemoveComponent(human.SearchComponent<HealthBarComponent>());
                    human.AddComponent(new HealthBarComponent(human,
                        Translations.Get(HumanType.Mage.ToString().ToLowerInvariant()), HealthBarType.Friendly));
                    human.AddComponent(new TalkComponent(human));
                    human.AddComponent(new GhostTownThoughtsComponent(human));
                    ((GhostTownPortal)Structure.WorldObject).NPC = human;
                }, Structure);
        }

        protected override GhostTownPortal Create(Vector3 Position, float Size)
        {
            return new GhostTownPortal(Position, StructureScale, RealmHandler.GhostTown,
                SpawnGhostTownPortalDesign.Position);
        }
    }

    public class GhostTownThoughtsComponent : ThoughtsComponent
    {
        public GhostTownThoughtsComponent(IEntity Entity, params object[] Parameters) : base(Entity, Parameters)
        {
        }

        protected override string ThoughtKeyword => "ghost_town_thought";
    }
}