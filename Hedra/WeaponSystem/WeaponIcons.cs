using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Core;

namespace Hedra.WeaponSystem
{
    public static class WeaponIcons
    {
        public static readonly uint SwordPrimaryAttack;
        public static readonly uint SwordSecondaryAttack;
        public static readonly uint KnifePrimaryAttack;
        public static readonly uint KnifeSecondaryAttack;
        public static readonly uint BowPrimaryAttack;
        public static readonly uint BowSecondaryAttack;
        public static readonly uint AxePrimaryAttack;
        public static readonly uint AxeSecondaryAttack;
        public static readonly uint HammerPrimaryAttack;
        public static readonly uint HammerSecondaryAttack;
        public static readonly uint DoubleBladesPrimaryAttack;
        public static readonly uint DoubleBladesSecondaryAttack;
        public static readonly uint KatarPrimaryAttack;
        public static readonly uint KatarSecondaryAttack;
        public static readonly uint ClawPrimaryAttack;
        public static readonly uint ClawSecondaryAttack;
        public static readonly uint StaffPrimaryAttack;
        public static readonly uint StaffSecondaryAttack;
        public static readonly uint DefaultAttack;

        static WeaponIcons()
        {
            SwordPrimaryAttack = Graphics2D.LoadFromAssets("Assets/Skills/Slash.png");
            SwordSecondaryAttack = Graphics2D.LoadFromAssets("Assets/Skills/Lunge.png");
            KnifePrimaryAttack = Graphics2D.LoadFromAssets("Assets/Skills/SlashKnife.png");
            KnifeSecondaryAttack = Graphics2D.LoadFromAssets("Assets/Skills/LungeKnife.png");
            BowPrimaryAttack = Graphics2D.LoadFromAssets("Assets/Skills/Shoot.png");
            BowSecondaryAttack = Graphics2D.LoadFromAssets("Assets/Skills/TripleShot.png");
            AxePrimaryAttack = Graphics2D.LoadFromAssets("Assets/Skills/SwingAxeIcon.png");
            AxeSecondaryAttack = Graphics2D.LoadFromAssets("Assets/Skills/SmashAxeIcon.png");
            HammerPrimaryAttack = Graphics2D.LoadFromAssets("Assets/Skills/SwingHammerIcon.png");
            HammerSecondaryAttack = Graphics2D.LoadFromAssets("Assets/Skills/SmashHammerIcon.png");
            DoubleBladesPrimaryAttack = Graphics2D.LoadFromAssets("Assets/Skills/BladesAttack1.png");
            DoubleBladesSecondaryAttack = Graphics2D.LoadFromAssets("Assets/Skills/BladesAttack2.png");
            KatarPrimaryAttack = Graphics2D.LoadFromAssets("Assets/Skills/KatarAttack1.png");
            KatarSecondaryAttack = Graphics2D.LoadFromAssets("Assets/Skills/KatarAttack2.png");
            ClawPrimaryAttack = Graphics2D.LoadFromAssets("Assets/Skills/ClawAttack1.png");
            ClawSecondaryAttack = Graphics2D.LoadFromAssets("Assets/Skills/ClawAttack2.png");
            StaffPrimaryAttack = Graphics2D.LoadFromAssets("Assets/Skills/StaffAttack1.png");
            StaffSecondaryAttack = Graphics2D.LoadFromAssets("Assets/Skills/StaffAttack2.png");
            DefaultAttack = Graphics2D.LoadFromAssets("Assets/Skills/HolderSkill.png");
            
            TextureRegistry.MarkStatic(SwordPrimaryAttack);
            TextureRegistry.MarkStatic(SwordSecondaryAttack);
            TextureRegistry.MarkStatic(KnifePrimaryAttack);
            TextureRegistry.MarkStatic(KnifeSecondaryAttack);
            TextureRegistry.MarkStatic(BowPrimaryAttack);
            TextureRegistry.MarkStatic(BowSecondaryAttack);
            TextureRegistry.MarkStatic(AxePrimaryAttack);
            TextureRegistry.MarkStatic(AxeSecondaryAttack);
            TextureRegistry.MarkStatic(HammerPrimaryAttack);
            TextureRegistry.MarkStatic(HammerSecondaryAttack);
            TextureRegistry.MarkStatic(DoubleBladesPrimaryAttack);
            TextureRegistry.MarkStatic(DoubleBladesSecondaryAttack);
            TextureRegistry.MarkStatic(KatarPrimaryAttack);
            TextureRegistry.MarkStatic(KatarSecondaryAttack);
            TextureRegistry.MarkStatic(ClawPrimaryAttack);
            TextureRegistry.MarkStatic(ClawSecondaryAttack);
            TextureRegistry.MarkStatic(StaffPrimaryAttack);
            TextureRegistry.MarkStatic(StaffSecondaryAttack);
            TextureRegistry.MarkStatic(DefaultAttack);
        }
    }
}