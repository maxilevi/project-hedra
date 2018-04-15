using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.Player.MapSystem
{
    public class MapBaseItem
    {
        public ObjectMesh Mesh { get; set; }

        public MapBaseItem(ObjectMesh Mesh)
        {
            this.Mesh = Mesh;
        }
    }
}
