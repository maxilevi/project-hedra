namespace Hedra.Engine.ModuleSystem.Templates
{
    public class AttackAnimationTemplate : AnimationTemplate
    {
        public string AttackEvent { get; set; } = "Mid";
        public float Chance { get; set; }
    }

    public enum AttackEvent {
        Start,
        Mid,
        End
    }
}
