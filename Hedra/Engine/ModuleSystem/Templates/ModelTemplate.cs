namespace Hedra.Engine.ModuleSystem.Templates
{
    public class ModelTemplate
    {
        public string Handler { get; set; }
        public bool AlignWithTerrain { get; set; } = true;
        public string Path { get; set; }
        public float Scale { get; set; }
        public bool IsUndead { get; set; }
        public AnimationTemplate[] IdleAnimations { get; set; }
        public AnimationTemplate[] WalkAnimations { get; set; }
        public AttackAnimationTemplate[] AttackAnimations { get; set; }
    }
}
