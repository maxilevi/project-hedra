using Hedra.API;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Rendering;
using System.Numerics;

namespace Hedra.Engine.ClassSystem
{
    public class MageDesign : ClassDesign
    {       
        public override Matrix4x4 HelmetPlacement { get; } = Matrix4x4.Identity;
        public override Matrix4x4 ChestplatePlacement { get; } = Matrix4x4.Identity;
        public override Matrix4x4 PantsMatrixPlacement { get; } = Matrix4x4.Identity;
        public override Matrix4x4 LeftBootPlacement { get; } = Matrix4x4.Identity;
        public override Matrix4x4 RightBootPlacement { get; } = Matrix4x4.Identity;
        public override Class Type => Class.Mage;
    }
}
