using System;
using System.Collections.Generic;
using System.Numerics;
using Hedra.Engine.Core;
using Hedra.Engine.WorldBuilding;

namespace Hedra.Engine.StructureSystem.VillageSystem.Templates
{
    public class StructureTemplate : IPositionable
    {
        private static readonly Dictionary<string, Type> Map = new Dictionary<string, Type>
        {
            { "Loom", typeof(Loom) },
            { "QuestBoard", typeof(QuestBoard) }
        };

        public string Type { get; set; }
        public Vector3 Position { get; set; }

        public static BaseStructure FromType(string Type, Vector3 Position)
        {
            return (BaseStructure)Activator.CreateInstance(Map[Type], Position);
        }
    }
}