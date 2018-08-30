﻿using Hedra.Engine.ItemSystem.WeaponSystem;
using Hedra.Engine.Player.Skills;

namespace Hedra.Engine.Player.ToolbarSystem
{
    public interface IToolbar
    {
        BaseSkill SkillAt(int Index);
        void Update();
        void UpdateView();
        void SetAttackType(Weapon CurrentWeapon);
        BaseSkill[] Skills { get; }
        bool DisableAttack { get; set; }
        bool BagEnabled { get; set; }
        bool Listen { get; set; }
        bool Show { get; set; }
        byte[] ToArray();
        void FromInformation(PlayerInformation Information);
    }
}