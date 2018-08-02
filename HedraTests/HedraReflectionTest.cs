using System;
using System.Reflection;
using Hedra.Engine.Rendering.UI;

namespace HedraTests
{
    public class HedraReflectionTest
    {
        public void ChangeField(string Name, object Value, Type Type, object Instance = null)
        {
            var flags = Instance == null ? BindingFlags.Static : BindingFlags.Instance;
            Type.GetField(Name, flags | BindingFlags.NonPublic | BindingFlags.Public)?.SetValue(Instance, Value);
        }
    }
}