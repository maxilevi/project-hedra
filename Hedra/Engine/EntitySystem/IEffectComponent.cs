namespace Hedra.Engine.EntitySystem
{
    public interface IEffectComponent
    {
        int Chance { get; set; }
        float Damage { get; set; }
        float Duration { get; set; }

        void Apply(Entity Victim, float Amount);
        void Dispose ();
    }
}