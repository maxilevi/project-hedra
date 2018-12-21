namespace Hedra.Engine.ModuleSystem.Templates
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