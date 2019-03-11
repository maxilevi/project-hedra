using System.Globalization;
using System.Linq;
using Hedra.Engine.Management;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;

namespace Hedra.Engine.SkillSystem.Mage.Necromancer
{
    public class SiphonBlood : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/SiphonBlood.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/NecromancerSiphonBlood.dae");
        private readonly Timer _timer = new Timer(.1f);
        private bool _canDo;

        protected override void OnAnimationMid()
        {
            base.OnAnimationMid();
            var entity = FindNearestVictim();
            if(entity == null) return;
            SpawnParticle(entity);
        }

        private void SpawnParticle(IEntity Victim)
        {
            var blood = new BloodProjectile(Victim, Victim.Position + OpenTK.Vector3.UnitY * 3f)
            {
                Direction = (Player.Position - Victim.Position).NormalizedFast(),
                UsePhysics = false,
                UseLight = true,
                Speed = .5f,
                IgnoreEntities = World.Entities.Where(E => E.SearchComponent<WarriorMinionComponent>()?.Owner == Player || E == Player.Pet.Pet).ToArray()
            };
            Victim.Damage(Damage, Player, out var xp);
            Player.XP += xp;
            blood.HitEventHandler += (_, __) =>
            {
                Player.Health += HealthBonus;
            };
            World.AddWorldObject(blood);
        }

        private IEntity FindNearestVictim()
        {
            return SkillUtils.GetNearest(Player, 96, E =>
            {
                if (E == Player.Pet.Pet || E.SearchComponent<WarriorMinionComponent>()?.Owner == Player) return false;
                return true;
            });
        }

        public override void Update()
        {
            base.Update();
            if (_timer.Tick())
            {
                _canDo = FindNearestVictim() != null;
            }
        }

        private float Damage => 22 + 22 * (Level / (float)MaxLevel);
        private float HealthBonus => Damage * .75f;
        protected override bool ShouldDisable => !_canDo;
        public override float ManaCost => 45;
        public override float MaxCooldown => 18 - 6 * (Level / (float) MaxLevel);
        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("siphon_blood_desc");
        public override string DisplayName => Translations.Get("siphon_blood_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("siphon_blood_damage_change", Damage.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("siphon_blood_health_change", HealthBonus.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}