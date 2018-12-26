namespace Hedra.Engine.ModuleSystem.Templates
{
    public class EffectTemplate
    {
        public string Name { get; set; }
        public float Chance { get; set; } = -1;
        public float Damage { get; set; } = -1;
        public float Duration { get; set; } = -1;

        public override string ToString()
        {
            return $"• {Name}: {Chance}%";
        }
    }
}
