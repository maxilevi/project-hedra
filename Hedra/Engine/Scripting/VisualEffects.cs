using Hedra.Engine.Core;
using Hedra.EntitySystem;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.Scripting
{
    public static class VisualEffects
    {
        private const string Name = "VisualEffects.py";
        
        public static void Outline(IHumanoid Humanoid, Vector4 Color, float Seconds)
        {
            Interpreter.GetFunction(Name, "outline").Invoke(Humanoid, Color, Seconds);
        }
    }
}