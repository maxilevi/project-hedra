namespace Hedra.Engine.ModuleSystem.Templates
{
    public class AnimationTemplate
    {
        public string Path { get; set; }
        public float Speed { get; set; }
        public string OnAnimationStart { get; set; }
        public string OnAnimationMid { get; set; }
        public string OnAnimationEnd { get; set; }
        public OnAnimationEvent OnAnimationProgress { get; set; }
    }

    public class OnAnimationEvent
    {
        public string Event { get; set; }
        public float Progress { get; set; }
    }
}
