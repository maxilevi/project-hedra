/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 05/07/2016
 * Time: 10:06 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Globalization;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.Sound;

namespace Hedra.Engine.SkillSystem.Mage
{
    /// <summary>
    /// Description of FireRelease.
    /// </summary>
    public class FireRelease : SwitchSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/FireRelease.png");
        protected override string AnimationPath => "Assets/Chr/Mage-FireRelease.dae";
        protected override SoundType SoundType => SoundType.None;
        private FireCone _cone;

        protected override void Activate()
        {
            User.Movement.CaptureMovement = false;
            _cone = FireCone.Create(User, 50,
                () => User.Mana > ManaPerSecond && Casting,
                () => User.Mana -= ManaPerSecond * Time.DeltaTime);
        }

        protected override void Deactivate()
        {
            User.Movement.CaptureMovement = true;
            _cone.Dispose();
            _cone = null;
        }

        protected override int MaxLevel => 25;
        private float Damage => 5f + 7 * (Level /(float) MaxLevel);
        private float ManaPerSecond => 26;
        public override string DisplayName => Translations.Get("fire_release_skill");
        public override string Description => Translations.Get("fire_release_desc");
        public override string[] Attributes => new []
        {
            Translations.Get("fire_release_damage_change", Damage.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("fire_release_drain_change", ManaPerSecond.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}
