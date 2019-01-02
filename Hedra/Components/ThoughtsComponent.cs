using System.Collections.Generic;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.EntitySystem;

namespace Hedra.Components
{
    public abstract class ThoughtsComponent : SingularComponent<ThoughtsComponent, IEntity>
    {
        protected abstract string ThoughtKeyword { get; }
        public Translation[] Thoughts { get; private set; }
        private object[] Parameters { get; }
        
        protected ThoughtsComponent(IEntity Entity, params object[] Parameters) : base(Entity)
        {
            this.Parameters = Parameters;
            UpdateThoughts();
        }

        protected void UpdateThoughts()
        {
            var thoughtList = new List<Translation>();
            var iterator = 0;
            while (Translations.Has($"{ThoughtKeyword}_{iterator}"))
            {
                thoughtList.Add(Translation.Create($"{ThoughtKeyword}_{iterator}", Parameters));
                iterator++;
            }
            Thoughts = thoughtList.ToArray();
        }
        
        public override void Update()
        {
        }
    }
}