using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class Well : CraftingStation
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
        
        public override CraftingSystem.CraftingStation StationType => CraftingSystem.CraftingStation.Well;

        protected override string CraftingMessage => Translations.Get("use_well");

        public override void Dispose()
        {
            base.Dispose();
            _light.Dispose();
            NPC?.Dispose();
        }
    }
}