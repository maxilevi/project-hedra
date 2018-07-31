namespace Hedra.Engine.ModuleSystem
{
    public class AnimationTemplate
    {
        public string Path { get; set; }
        public float Speed { get; set; }
        public string OnAnimationStart { get; set; }
        public string OnAnimationMid { get; set; }
        public string OnAnimationEnd { get; set; }
    }
}
