/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 05/07/2016
 * Time: 10:06 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Sound;

namespace Hedra.Engine.SkillSystem.Mage
{
    /// <summary>
    /// Description of FireRelease.
    /// </summary>
    public class FireRelease : SwitchSkill
    {
        private const int ManaPerSecond = 6;
        private const int DamagePerSecond = 5;
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/FireRelease.png");
        public override string DisplayName => "Fire Release";
        protected override string AnimationPath => "Assets/Chr/Mage-FireRelease.dae";
        protected override SoundType SoundType => SoundType.ArrowHit;

        protected override void Activate()
        {
            Player.Movement.CaptureMovement = false;
            FireCone.Create(Player, 50,
                () => Player.Mana > ManaPerSecond && Casting,
                () => Player.Mana -= ManaPerSecond * Time.DeltaTime);
        }

        protected override void Deactivate()
        {
            Player.Movement.CaptureMovement = true;
        }

        protected override int MaxLevel => 25;
        public override string Description => "A cone of flames to burn your enemies.";
    }
}
