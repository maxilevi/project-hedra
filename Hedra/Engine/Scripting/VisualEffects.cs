using Hedra.Engine.Core;
using Hedra.EntitySystem;
using System.Numerics;

namespace Hedra.Engine.Scripting
{
    public static class VisualEffects
    {
        private const string Name = "VisualEffects.py";
        
        public static void Outline(IEntity Humanoid, Vector4 Color, float Seconds)
        {
            Interpreter.GetFunction(Name, "outline").Invoke(Humanoid, Color, Seconds);
        }
        
        public static void SetOutline(IEntity Humanoid, Vector4 Color, bool State)
        {
            Interpreter.GetFunction(Name, "set_outline").Invoke(Humanoid, Color, State);
        }
    }
}