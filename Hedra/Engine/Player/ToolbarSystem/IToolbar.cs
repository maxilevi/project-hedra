using Hedra.Engine.SkillSystem;
using Hedra.WeaponSystem;

namespace Hedra.Engine.Player.ToolbarSystem
{
    public interface IToolbar : ISerializableHandler
    {
        AbstractBaseSkill[] Skills { get; }
        bool DisableAttack { get; set; }
        bool BagEnabled { get; set; }
        bool PassiveEffectsEnabled { get; set; }
        bool Listen { get; set; }
        bool Show { get; set; }
        AbstractBaseSkill SkillAt(int Index);
        void Update();
        void UpdateView();
        void SetAttackType(Weapon CurrentWeapon);
        void ResetCooldowns();
        void ResetSkills();
        void UpdateSkills();
        void Empty();
    }
}