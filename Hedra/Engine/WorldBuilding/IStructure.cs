using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Hedra.Engine.WorldBuilding
{
    public interface IStructure
    {
        BaseStructure[] Children { get;}
        Vector3 Position { get; set; }
        bool Disposed { get; }
        void AddChildren(params BaseStructure[] Children);
        void Dispose();
    }
}
