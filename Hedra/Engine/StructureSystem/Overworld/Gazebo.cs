using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class Gazebo : BaseStructure, IQuestStructure
    {
        private readonly WorldLight _light;
        
        public Gazebo(Vector3 Position, float Radius) : base(Position)
        {
            _light = new WorldLight(Position)
            {
                Radius = Radius,
                LightColor = WorldLight.DefaultColor
            };
        }

        public IHumanoid NPC { get; set; }

        public override void Dispose()
        {
            base.Dispose();
            NPC?.Dispose();
            _light.Dispose();
        }
    }
}