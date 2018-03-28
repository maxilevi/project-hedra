namespace Hedra.Engine.EntitySystem
{
    public interface IEffectComponent
    {
        int Chance { get; set; }
        float TotalStrength { get; set; }
        float BaseTime { get; set; }

        void Apply(Entity Victim, float Amount);
        void Dispose ();
    }
}