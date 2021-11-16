using Hedra.EntitySystem;

namespace Hedra.Engine.EntitySystem
{
    public class ArmorBonusComponent : Component<IHumanoid>
    {
        private readonly float _armorBonus;

        public ArmorBonusComponent(IHumanoid Parent, float Health) : base(Parent)
        {
            _armorBonus = Health;
            Parent.Armor += _armorBonus;
        }

        public override void Update()
        {
        }

        public override void Dispose()
        {
            if (Disposed) return;
            Parent.Armor -= _armorBonus;
            base.Dispose();
        }
    }
}