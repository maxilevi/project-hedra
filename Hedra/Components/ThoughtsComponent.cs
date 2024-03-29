using System.Collections.Generic;
using Hedra.Engine.Localization;
using Hedra.EntitySystem;
using Hedra.Localization;

namespace Hedra.Components
{
    public abstract class ThoughtsComponent : SingularComponent<ThoughtsComponent, IEntity>
    {
        protected ThoughtsComponent(IEntity Entity, params object[] Parameters) : base(Entity)
        {
            this.Parameters = Parameters;
            UpdateThoughts();
        }

        protected abstract string ThoughtKeyword { get; }
        public Translation[] Thoughts { get; private set; }
        private object[] Parameters { get; }

        public virtual Translation[] BeforeDialog { get; } = new Translation[0];
        public virtual Translation[] AfterDialog { get; } = new Translation[0];

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