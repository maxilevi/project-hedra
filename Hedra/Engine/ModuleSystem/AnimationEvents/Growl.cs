using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Sound;
using OpenTK;

namespace Hedra.Engine.ModuleSystem.AnimationEvents
{
    public class Growl : AnimationEvent
    {
        public Growl(Entity Parent) : base(Parent) { }

        public override void Build()
        {
            var position = Parent.Position + Parent.Orientation * Parent.Model.Scale * 6f;
            World.HighlightArea(position, Vector4.One * .05f, 64f, 3.0f);

            var entities = World.Entities;
            foreach (var entity in entities)
            {
                if(entity == Parent || entity.IsStatic) continue;
                var range = 1 - Mathf.Clamp((position - entity.Position).Xz.LengthFast / 32f, 0, 1);
                entity.AddComponent(new SlowingComponent(entity, Parent, 3f, 30f * range));
                entity.ShowIcon(CacheItem.FearIcon, 3f);
            }
            SoundManager.PlaySound(SoundType.GorillaGrowl, position, false, 1f, 5f);
        }
    }
}
