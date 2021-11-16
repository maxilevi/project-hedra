using Hedra.Components;

namespace Hedra.Engine.ModuleSystem.Templates
{
    public class HumanoidConfiguration
    {
        public HumanoidConfiguration(HealthBarType Type)
        {
            this.Type = Type;
        }

        public HealthBarType Type { get; set; }
    }
}