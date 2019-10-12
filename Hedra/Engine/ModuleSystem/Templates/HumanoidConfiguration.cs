using Hedra.Components;
using Hedra.Engine.EntitySystem;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.ModuleSystem.Templates
{
    public class HumanoidConfiguration
    {
        public HealthBarType Type { get; set; }

        public HumanoidConfiguration(HealthBarType Type)
        {
            this.Type = Type;
        }
    }
}
