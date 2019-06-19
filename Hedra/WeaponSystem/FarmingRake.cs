using Hedra.Engine;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.WeaponSystem
{
    public class FarmingRake : Tool
    {
        public FarmingRake(VertexData Contents) : base(Contents)
        {
        }
        
        protected override float PrimarySpeed => 0.75f;
        protected override string[] PrimaryAnimationsNames => new []
        {
            "Assets/Chr/WarriorRake.dae",
        };
        protected override float SecondarySpeed => 0.75f;

        protected override string[] SecondaryAnimationsNames => new []
        {
            "Assets/Chr/WarriorRake.dae"
        };
    }
}