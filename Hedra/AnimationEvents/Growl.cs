using Hedra.Components.Effects;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ModuleSystem.AnimationEvents;
using Hedra.Engine.SkillSystem;
using Hedra.Engine.Sound;
using Hedra.EntitySystem;
using Hedra.Sound;
using OpenToolkit.Mathematics;

namespace Hedra.AnimationEvents
{
    public class Growl : AnimationEvent
    {
        public Growl(ISkilledAnimableEntity Parent) : base(Parent) { }

        public override void Build()
        {
            var position = Parent.Position + Parent.Orientation * Parent.Model.Scale * 6f;
            World.HighlightArea(position, Vector4.One * .05f, 64f, 3.0f);

            var entities = World.Entities;
            foreach (var entity in entities)
            {
                if(entity == Parent || entity.IsStatic) continue;
                var range = 1 - Mathf.Clamp((position - entity.Position).Xz.LengthFast / 32f, 0, 1);
                if (range < 0.005f) continue;
                entity.AddComponent(new SlowingComponent(entity, Parent, 3f, 100 - 30f * range - 25f));               
                entity.ShowIcon(CacheItem.FearIcon, 3f);
            }
            SoundPlayer.PlaySound(SoundType.GorillaGrowl, position, false, 1f, 5f);
        }
    }
}
