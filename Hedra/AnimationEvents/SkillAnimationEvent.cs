using Hedra.Core;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.SkillSystem;
using Hedra.EntitySystem;

namespace Hedra.AnimationEvents
{
    public abstract class SkillAnimationEvent<T> : AnimationEvent where T : BaseSkill<ISkilledAnimableEntity>, new()
    {
        private readonly T _skill;
        
        protected SkillAnimationEvent(ISkilledAnimableEntity Parent) : base(Parent)
        {
            _skill = new T();
            _skill.Initialize(Parent);
            _skill.Level = Level;
        }

        public override void Build()
        {
            base.Build();
            _skill.Use();
            TaskScheduler.When(
                () => !_skill.Casting,
                Dispose
            );
        }

        public override void Dispose()
        {
            base.Dispose();
            _skill.Dispose();
        }

        protected virtual int Level => 1;
    }
}