using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;
using Hedra.Engine.Scenes;

namespace Hedra.Engine.ModuleSystem
{
    internal interface IEnemyFactory
    {
        string Name { get; set; }

        void Apply(Entity Mob);
    }
}