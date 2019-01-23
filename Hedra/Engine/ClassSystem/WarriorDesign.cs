using Hedra.API;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.ClassSystem
{
    public class WarriorDesign : ClassDesign
    {
        public override Matrix4 HelmetPlacement { get; } = Matrix4.Identity;
        public override Matrix4 ChestplatePlacement { get; } =
            Matrix4.CreateScale(new Vector3(1.3f, 1.2f, 1));
        public override Matrix4 PantsMatrixPlacement { get; } = Matrix4.Identity;
        public override Matrix4 LeftBootPlacement { get; } = Matrix4.Identity;
        public override Matrix4 RightBootPlacement { get; } = Matrix4.Identity;
        public override Class Type => Class.Warrior;
    }
}
