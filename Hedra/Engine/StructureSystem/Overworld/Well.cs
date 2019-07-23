using System;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Localization;
using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class Well : CraftingStation, IQuestStructure
    {
        private readonly WorldLight _light;
        public IHumanoid NPC { get; set; }
        
        public Well(Vector3 Position, float Radius) : base(Position)
        {
            _light = new WorldLight(Position)
            {
                Radius = Radius,
                LightColor = HandLamp.LightColor
            };
        }
        
        public override Crafting.CraftingStation StationType => Crafting.CraftingStation.Well;

        protected override string CraftingMessage => Translations.Get("use_well");

        public override void Dispose()
        {
            base.Dispose();
            _light.Dispose();
            NPC?.Dispose();
        }
    }
}