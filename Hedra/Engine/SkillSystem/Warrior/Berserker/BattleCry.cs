using System.Globalization;
using Hedra.Components.Effects;
using Hedra.Core;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Warrior.Berserker
{
    public class BattleCry : SingleAnimationSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/BattleCry.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/WarriorBattleCry.dae");
        protected override bool EquipWeapons => false;

        protected override void OnAnimationMid()
        {
            World.HighlightArea(Player.Position, Vector4.One * .05f, Range, Duration);
            SkillUtils.DoNearby(Player, Range, -1, (E, F) =>
            {
                if(E == Player || E.IsStatic) return;
                var range = 1 - Mathf.Clamp((Player.Position - E.Position).Xz.LengthFast / Range, 0, 1);
                if (range < 0.005f) return;
                E.AddComponent(new FearComponent(E, Player, Duration, 100 - Slowness * range));
            });
            SoundPlayer.PlaySound(SoundType.GorillaGrowl, Player.Position);
        }

        protected override int MaxLevel => 15;
        private float Range => 24;
        private float Duration => 4 + 3 * (Level / (float) MaxLevel);
        private float Slowness => 25 + 30 * (Level / (float) MaxLevel);
        public override float ManaCost => 0;
        public override float MaxCooldown => 32f - 7f * (Level / (float) MaxLevel);
        public override string Description => Translations.Get("battle_cry_desc");
        public override string DisplayName => Translations.Get("battle_cry_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("battle_cry_duration_change", Duration.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("battle_cry_slowness_change", (int) Slowness)
        };
    }
}