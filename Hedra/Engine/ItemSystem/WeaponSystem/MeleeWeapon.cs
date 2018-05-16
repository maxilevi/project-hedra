﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.ItemSystem.WeaponSystem
{
    public abstract class MeleeWeapon : Weapon
    {
        protected MeleeWeapon(VertexData MeshData) : base(MeshData) {}
    }
}
