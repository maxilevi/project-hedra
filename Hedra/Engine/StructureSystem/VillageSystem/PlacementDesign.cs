﻿using System.Linq;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public class PlacementDesign
    {
        public Vector3 Position { get; private set; }
        public FarmParameters[] Farms { get; set; } = new FarmParameters[0];
        public BuildingParameters[] Houses { get; set; } = new BuildingParameters[0];
        public BlacksmithParameters[] Blacksmith { get; set; } = new BlacksmithParameters[0];
        public BuildingParameters[] Stables { get; set; } = new BuildingParameters[0];
        public BuildingParameters[] Markets { get; set; } = new BuildingParameters[0];

        public IBuildingParameters[] Parameters => Farms.Concat<IBuildingParameters>(Houses)
            .Concat(Blacksmith).Concat(Stables).Concat(Markets).ToArray();

        public void Translate(Vector3 Translation)
        {
            var parameters = Parameters;
            for (var i = 0; i < parameters.Length; i++)
            {
                parameters[i].Position += Translation;
            }
            Position += Translation;
        }
    }
}