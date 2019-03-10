using Hedra.Engine.EntitySystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Mage.Necromancer
{
    public class RaiseSkeleton : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/RaiseSkeletons.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/NecromancerRaiseSkeleton.dae");
        private int _currentMinions;
        
        protected override void OnAnimationEnd()
        {
            base.OnAnimationEnd();
            if(_currentMinions < MaxMinions)
                SpawnMinion();
        }

        private void SpawnMinion()
        {
            var skeleton = World.WorldBuilding.SpawnHumanoid(HumanType.Skeleton, Player.Position + Player.Orientation * 16);
            skeleton.AddComponent(new WarriorMinionComponent(skeleton, Player));
            skeleton.SetWeapon(ItemPool.Grab(ItemTier.Uncommon).Weapon);
            skeleton.SearchComponent<DamageComponent>().OnDamageEvent += A =>
            {
                if (A.Victim.IsDead)
                    _currentMinions--;
            };
        }

        private int MaxMinions => 1 + (int) (4 * (Level / (float) MaxLevel));
        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("raise_skeleton_desc");
        public override string DisplayName => Translations.Get("raise_skeleton_skill");
    }
}