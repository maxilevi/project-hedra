using Hedra.Engine.EntitySystem;
using Hedra.Engine.EntitySystem.BossSystem;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Components
{
    public class BossXPMessageComponent : EntityComponent
    {
        public BossXPMessageComponent(IEntity Entity) : base(Entity)
        {
            var dmgComponent = Entity.SearchComponent<DamageComponent>();
            dmgComponent.OnDamageEvent += delegate(DamageEventArgs Args)
            {
                if (!(Args.Victim.Health <= 0)) return;

                GameManager.Player.MessageDispatcher.ShowMessage(Translations.Get("boss_get_xp", (int) dmgComponent.XpToGive), 3f, Colors.Violet.ToColor());
                Entity.SearchComponent<BossHealthBarComponent>().Enabled = false;
            };
        }

        public override void Update()
        {
        }
    }
}