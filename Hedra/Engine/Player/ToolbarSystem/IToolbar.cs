using Hedra.Engine.SkillSystem;
using Hedra.WeaponSystem;

namespace Hedra.Engine.Player.ToolbarSystem
{
    public interface IToolbar : ISerializableHandler
    {
        BaseSkill SkillAt(int Index);
        void Update();
        void UpdateView();
        void SetAttackType(Weapon CurrentWeapon);
        BaseSkill[] Skills { get; }
        bool DisableAttack { get; set; }
        bool BagEnabled { get; set; }
        bool PassiveEffectsEnabled { get; set; }
        bool Listen { get; set; }
        bool Show { get; set; }
        void ResetCooldowns();
        void ResetSkills();
    }
}