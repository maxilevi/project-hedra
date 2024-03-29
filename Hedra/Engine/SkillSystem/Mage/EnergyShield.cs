using System.Globalization;
using System.Numerics;
using Hedra.Components.Effects;
using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Rendering;

namespace Hedra.Engine.SkillSystem.Mage
{
    public class EnergyShield : ActivateDurationSkill<IPlayer>
    {
        private AttackResistanceBonusComponent _bonusResistance;
        private EnergyShieldComponent _shield;
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/EnergyShield.png");

        protected override int MaxLevel => 15;
        public override float ManaCost => 50f + 30f * (Level / (float)MaxLevel);
        protected override float Duration => 6 + 9 * (Level / (float)MaxLevel);
        protected override float CooldownDuration => 24 - 8 * (Level / (float)MaxLevel);
        private float ResistanceChange => .2f + .55f * (Level / (float)MaxLevel);
        public override string Description => Translations.Get("energy_shield_desc");
        public override string DisplayName => Translations.Get("energy_shield_skill");

        public override string[] Attributes => new[]
        {
            Translations.Get("energy_shield_duration_change", Duration.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("energy_shield_resistance_change", (int)(ResistanceChange * 100))
        };

        protected override void DoEnable()
        {
            User.AddComponent(_shield = new EnergyShieldComponent(User));
            User.AddComponent(_bonusResistance =
                new AttackResistanceBonusComponent(User, User.AttackResistance * ResistanceChange));
        }

        protected override void DoDisable()
        {
            User.RemoveComponent(_shield);
            User.RemoveComponent(_bonusResistance);
        }

        private class EnergyShieldComponent : Component<IHumanoid>
        {
            private static readonly VertexData ShieldMesh;
            private readonly ObjectMesh _shieldObject;

            static EnergyShieldComponent()
            {
                ShieldMesh = AssetManager.PLYLoader("Assets/Env/EnergyShield.ply", Vector3.One);
            }

            public EnergyShieldComponent(IHumanoid Entity) : base(Entity)
            {
                _shieldObject =
                    ObjectMesh.FromVertexData(ShieldMesh.Clone().Scale(Vector3.One * Parent.Model.Height * .45f));
                _shieldObject.Alpha = .4f;
                _shieldObject.Enabled = true;
                _shieldObject.Outline = Parent.Model.Outline = true;
                _shieldObject.OutlineColor = Parent.Model.OutlineColor = Colors.LightBlue * .4f;
                _shieldObject.ApplySSAO = false;
                DrawManager.RemoveObjectMesh(_shieldObject);
                DrawManager.AddTransparent(_shieldObject);
            }

            public override void Update()
            {
                _shieldObject.Position = Parent.Model.ModelPosition + Vector3.UnitY * Parent.Model.Height * .5f;
                _shieldObject.Rotation += Vector3.One * Time.DeltaTime * 2000f;
            }

            public override void Dispose()
            {
                base.Dispose();
                Parent.Model.Outline = false;
                DrawManager.RemoveTransparent(_shieldObject);
                _shieldObject.Dispose();
            }
        }
    }
}