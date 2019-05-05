using System;
using Hedra.Components;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.GhostTown
{
    public class GhostTownPortalDesign : SimpleFindableStructureDesign<GhostTownPortal>
    {
        public override int PlateauRadius => 140;
        public override VertexData Icon { get; } = CacheManager.GetModel(CacheItem.PortalIcon);
        public override VertexData QuestIcon => Icon;
        protected override int StructureChance => StructureGrid.GhostTownPortalChance;
        protected override CacheItem? Cache => CacheItem.Portal;
        protected override Vector3 Scale => Vector3.One * 10;
        protected virtual bool SpawnNPC => true;

        protected override void DoBuild(CollidableStructure Structure, Matrix4 Rotation, Matrix4 Translation, Random Rng)
        {
            base.DoBuild(Structure, Rotation, Translation, Rng);
            if(!SpawnNPC) return;
            DoWhenChunkReady(Vector3.TransformPosition(-Vector3.UnitZ * 5 * Scale - Vector3.UnitX * 2 * Scale, Rotation * Translation), P =>
            {
                var human = World.WorldBuilding.SpawnHumanoid(HumanType.Mage, P);
                human.Physics.UsePhysics = false;
                human.Physics.TargetPosition = P;
                human.SearchComponent<DamageComponent>().Immune = true;
                human.RemoveComponent(human.SearchComponent<HealthBarComponent>());
                human.AddComponent(new HealthBarComponent(human, Translations.Get(HumanType.Mage.ToString().ToLowerInvariant()), HealthBarType.Friendly));
                human.AddComponent(new TalkComponent(human));
                human.AddComponent(new GhostTownThoughtsComponent(human));
                ((GhostTownPortal) Structure.WorldObject).NPC = human;
            }, Structure);
        }

        protected override GhostTownPortal Create(Vector3 Position, float Size)
        {
            return new GhostTownPortal(Position, Scale, RealmHandler.GhostTown, SpawnGhostTownPortalDesign.Position);
        }
        
        public override string DisplayName => Translations.Get("structure_portal");
    }

    public class GhostTownThoughtsComponent : ThoughtsComponent
    {
        public GhostTownThoughtsComponent(IEntity Entity, params object[] Parameters) : base(Entity, Parameters)
        {
        }

        protected override string ThoughtKeyword => "ghost_town_thought";
    }
}