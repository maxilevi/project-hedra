using System.Numerics;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Localization;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class Well : CraftingStation, IQuestStructure
    {
        private readonly WorldLight _light;

        public Well(Vector3 Position, float Radius) : base(Position)
        {
            _light = new WorldLight(Position)
            {
                Radius = Radius,
                LightColor = WorldLight.DefaultColor
            };
        }

        public override Crafting.CraftingStation StationType => Crafting.CraftingStation.Well;

        protected override string CraftingMessage => Translations.Get("use_well");
        public IHumanoid NPC { get; set; }

        public override void Dispose()
        {
            base.Dispose();
            _light.Dispose();
            NPC?.Dispose();
        }
    }
}