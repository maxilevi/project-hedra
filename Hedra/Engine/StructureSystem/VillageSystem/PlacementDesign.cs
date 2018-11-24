using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public class PlacementDesign
    {
        public Vector3 Position { get; private set; }
        public List<FarmParameters> Farms { get; set; } = new List<FarmParameters>();
        public List<HouseParameters> Neighbourhoods { get; set; } = new List<HouseParameters>();
        public List<BlacksmithParameters> Blacksmith { get; set; } = new List<BlacksmithParameters>();
        public List<BuildingParameters> Stables { get; set; } = new List<BuildingParameters>();
        public List<MarketParameters> Markets { get; set; } = new List<MarketParameters>();

        public IBuildingParameters[] Parameters => Farms.Concat<IBuildingParameters>(Neighbourhoods)
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