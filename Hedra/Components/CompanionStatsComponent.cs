using Hedra.Core;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Scripting;
using Hedra.EntitySystem;

namespace Hedra.Components
{
    public class CompanionStatsComponent : EntityComponent
    {
        private const string LibraryName = "Companion.py";
        private static readonly string NameAttributeName = Interpreter.GetMember<string>(LibraryName, "NAME_ATTRIB_NAME");
        private static readonly string XPAttributeName = Interpreter.GetMember<string>(LibraryName, "XP_ATTRIB_NAME");
        private static readonly string HealthAttributeName = Interpreter.GetMember<string>(LibraryName, "HEALTH_ATTRIB_NAME");
        private readonly Item _storage;
        private readonly Timer _saveTimer;
        private readonly float _baseAttackDamage;
        private int _level = 1;

        public CompanionStatsComponent(IEntity Entity, Item Storage) : base(Entity)
        {
            _storage = Storage;
            _saveTimer = new Timer(1);
            _baseAttackDamage = Parent.AttackDamage;
            /* Force update the xp */
            XP = XP;
            Parent.Health = _storage.GetAttribute<float>(HealthAttributeName);
        }

        public override void Update()
        {
            if(_saveTimer.Tick())
                _storage.SetAttribute(HealthAttributeName, Parent.Health);
            if(!Parent.SearchComponent<DamageComponent>().HasBeenAttacked && !Parent.IsAttacking)
                Parent.Health += HealthRegen * Time.DeltaTime;
        }

        public string Name
        {
            get => _storage.GetAttribute<string>(NameAttributeName);
            set => _storage.SetAttribute(NameAttributeName, value);
        }

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

        public int Level
        {
            get => _level;
            set
            {
                _level = value;
                Parent.AttackDamage = _baseAttackDamage * (1 + _level * .4f);
            }
        }

        public float MaxXP => Humanoid.MaxLevel == Level ? 0 : ClassDesign.XPFormula(Level);
        private float HealthRegen => 2;
    }
}