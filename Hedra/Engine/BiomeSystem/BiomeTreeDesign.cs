using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.TreeSystem;

namespace Hedra.Engine.BiomeSystem
{
    internal abstract class BiomeTreeDesign
    {
        public abstract TreeDesign[] AvailableTypes { get; }
    }
}
