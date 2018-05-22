namespace Hedra.Engine.ModuleSystem
{
    public class ModelTemplate
    {
        public bool AlignWithTerrain { get; set; } = true;
        public string Path { get; set; }
        public float Scale { get; set; }
        public AnimationTemplate IdleAnimation { get; set; }
        public AnimationTemplate WalkAnimation { get; set; }
        public AnimationTemplate[] AttackAnimations { get; set; }
    }
}
