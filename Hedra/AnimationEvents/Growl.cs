using System.Numerics;
using Hedra.Components.Effects;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.SkillSystem;
using Hedra.Numerics;
using Hedra.Sound;

namespace Hedra.AnimationEvents
{
    public class Growl : AnimationEvent
    {
        public Growl(ISkilledAnimableEntity Parent) : base(Parent)
        {
        }

        public override void Build()
        {
            var position = Parent.Position + Parent.Orientation * Parent.Model.Scale * 6f;
            World.HighlightArea(position, Vector4.One * .05f, 64f, 3.0f);

            var entities = World.Entities;
            foreach (var entity in entities)
            {
                if (entity == Parent || entity.IsStatic) continue;
                var range = 1 - Mathf.Clamp((position - entity.Position).Xz().LengthFast() / 32f, 0, 1);
                if (range < 0.005f) continue;
                entity.AddComponent(new SlowingComponent(entity, Parent, 3f, 100 - 30f * range - 25f));
                entity.ShowIcon(CacheItem.FearIcon, 3f);
            }

            SoundPlayer.PlaySound(SoundType.GorillaGrowl, position, false, 1f, 5f);
        }
    }
}