using System.Collections.Generic;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.EntitySystem;

namespace Hedra.Components
{
    public abstract class ThoughtsComponent : EntityComponent
    {
        protected abstract string ThoughtKeyword { get; }
        public Translation[] Thoughts { get; }
        
        protected ThoughtsComponent(IEntity Entity) : base(Entity)
        {
            var thoughtList = new List<Translation>();
            var iterator = 0;
            while (Translations.Has($"{ThoughtKeyword}_{iterator}"))
            {
                thoughtList.Add(Translation.Create($"{ThoughtKeyword}_{iterator}"));
                iterator++;
            }
            Thoughts = thoughtList.ToArray();
        }

        public override void Update()
        {
        }
    }
}