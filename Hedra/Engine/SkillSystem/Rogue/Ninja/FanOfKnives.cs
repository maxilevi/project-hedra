using System.Globalization;
using System.Net.NetworkInformation;
using Hedra.Components.Effects;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.Sound;
using Hedra.WorldObjects;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Engine.SkillSystem.Rogue.Ninja
{
    public sealed class FanOfKnives : SingleAnimationSkill<IPlayer>
    {
        private static readonly VertexData KnifeModel = AssetManager.PLYLoader("Assets/Items/Ammo/Kunai.ply", Vector3.One);
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/FanOfKnives.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/RogueKnifeThrow.dae");
        protected override bool EquipWeapons => false;
        protected override float AnimationSpeed => 1.25f;

        public FanOfKnives()
        {
            SkillAnimation.RegisterOnProgressEvent(.25f, Animation =>
            {
                ThrowAll();
            });
        }
        
        private void ThrowAll()
        {
            var direction = User.View.LookingDirection;
            var left = direction.Xz().PerpendicularLeft().ToVector3() + Vector3.UnitY * direction.Y;
            var right = direction.Xz().PerpendicularRight().ToVector3() + Vector3.UnitY * direction.Y;
            Throw(left * .25f + direction * .75f);
            TaskScheduler.After(.15f,
                () => Throw(direction)
            );
            TaskScheduler.After(.25f,
                () => Throw(right * .25f + direction * .75f)
            );
        }

        private void Throw(Vector3 Direction)
        {
            var weaponData = KnifeModel.Clone().Scale(Vector3.One).RotateX(90);
            var startingLocation = User.Model.LeftWeaponPosition;

            var weaponProj = new Projectile(User, startingLocation, weaponData)
            {
                Propulsion = Direction * 3.0f,
                Lifetime = 5f
            };
            weaponProj.HitEventHandler += delegate(Projectile Sender, IEntity Hit)
            {
                Hit.Damage(Damage, User, out var exp);
                User.XP += exp;
                if(Utils.Rng.NextFloat() < BleedChance && Hit.SearchComponent<BleedingComponent>() == null)
                    Hit.AddComponent(new BleedingComponent(Hit, User, 5f, Damage * .15f));
            };
            SoundPlayer.PlaySound(SoundType.BowSound, User.Position, false,  1f + Utils.Rng.NextFloat() * .2f - .1f, 2.5f);
            World.AddWorldObject(weaponProj);
        }

        protected override int MaxLevel => 15;
        private float Damage => 12 + Level * 1.5f;
        private float BleedChance => .1f + .2f * (Level / (float)MaxLevel);
        public override float ManaCost => 15;
        public override float MaxCooldown => 12;
        public override string Description => Translations.Get("fan_of_knives_desc");
        public override string DisplayName => Translations.Get("fan_of_knives_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("fan_of_knives_damage_change", Damage.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("fan_of_knives_bleed_change", (int) (BleedChance * 100))
        };
    }
}