using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.ItemSystem.WeaponSystem
{
    public abstract class RangedWeapon : Weapon
    {
        public override bool IsMelee { get; protected set; } = false;
        protected RangedWeapon(VertexData MeshData) : base(MeshData)
        {
        }
    }
}
