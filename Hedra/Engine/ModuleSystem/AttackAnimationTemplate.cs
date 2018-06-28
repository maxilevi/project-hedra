namespace Hedra.Engine.ModuleSystem
{
    internal class AttackAnimationTemplate : AnimationTemplate
    {
        public string AttackEvent { get; set; } = "Mid";
    }

    public enum AttackEvent {
        Start,
        Mid,
        End
    }
}
