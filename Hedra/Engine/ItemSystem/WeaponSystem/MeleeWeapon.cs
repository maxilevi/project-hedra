using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.ItemSystem.WeaponSystem
{
    public abstract class MeleeWeapon : Weapon
    {
        public override bool IsMelee { get; protected set; } = true;
        protected MeleeWeapon(VertexData MeshData) : base(MeshData) {}
    }
}
