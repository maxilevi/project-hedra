using System;
using System.Collections.Generic;
using Hedra.Engine.Core;
using Hedra.Engine.WorldBuilding;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.StructureSystem.VillageSystem.Templates
{
        
    public class StructureTemplate : IPositionable
    {
        public string Type { get; set; }
        public Vector3 Position { get; set; }

        private static readonly Dictionary<string, Type> Map = new Dictionary<string, Type>
        {
            {"Loom", typeof(Loom)},
            {"QuestBoard", typeof(QuestBoard)}
        };
        
        public static BaseStructure FromType(string Type, Vector3 Position)
        {
            return (BaseStructure) Activator.CreateInstance(Map[Type], Position);
        }
    }
}