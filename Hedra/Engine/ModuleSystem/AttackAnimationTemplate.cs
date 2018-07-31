namespace Hedra.Engine.ModuleSystem
{
    public class AttackAnimationTemplate : AnimationTemplate
    {
        public string AttackEvent { get; set; } = "Mid";
    }

    public enum AttackEvent {
        Start,
        Mid,
        End
    }
}
