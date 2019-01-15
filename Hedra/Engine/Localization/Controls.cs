using OpenTK.Input;

namespace Hedra.Engine.Localization
{
    public delegate void OnControlsChangedEvent();
    
    public class Controls
    {
        public static event OnControlsChangedEvent OnControlsChanged;
        public static Key InventoryOpen => Key.I;
        public static Key Interact => Key.E;
        public static Key Forward => Key.W;
        public static Key Leftward => Key.A;
        public static Key Backward => Key.S;
        public static Key Rightward => Key.D;
        public static Key Climb => Key.ControlLeft;
        public static Key Jump => Key.Space;
        public static Key Descend => Key.ShiftLeft;
        public static Key Handlamp => Key.F;
        public static Key Respawn = Key.R;
        public static Key Skilltree => Key.X;
        public static Key Map => Key.M;
        public static Key Eat => Key.Q;
        public static Key SpecialItem => Key.G;
        public static Key Respect => Key.F;
        public static Key Crafting => Key.C;
        public static Key QuestLog => Key.T;
    }
}