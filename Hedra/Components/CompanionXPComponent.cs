using Hedra.Engine.ClassSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.EntitySystem;

namespace Hedra.Components
{
    public class CompanionXPComponent : EntityComponent
    {
        private const string XPAttributeName = "PetXp";
        private readonly Item _storage;

        public CompanionXPComponent(IEntity Entity, Item Storage) : base(Entity)
        {
            _storage = Storage;
            /* Force update the xp */
            XP = XP;
        }

        public override void Update() {}

        public float XP
        {
            get => _storage.GetAttribute<float>(XPAttributeName);
            set
            {
                while (value >= MaxXP)
                {
                    value -= MaxXP;
                    Level++;
                }
                _storage.SetAttribute(XPAttributeName, value);
            }
        }

        public int Level { get; private set; } = 1;

        public float MaxXP
        {
            get => Humanoid.MaxLevel == Level ? 0 : ClassDesign.XPFormula(Level);
        }
    }
}