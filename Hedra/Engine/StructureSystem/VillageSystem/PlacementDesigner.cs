using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Hedra.Engine.Generation;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.StructureSystem.VillageSystem.Layout;
using Hedra.Engine.StructureSystem.VillageSystem.Placers;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public abstract class PlacementDesigner : IPlacementDesigner
    {
        protected VillageRoot Root { get; private set; }
        protected Random Rng { get; private set; }
        protected VillageConfiguration Config { get; private set; }
        protected FarmPlacer FarmPlacer { get; private set; }
        protected BlacksmithPlacer BlacksmithPlacer { get; private set; }
        protected NeighbourhoodPlacer NeighbourhoodPlacer { get; private set; }
        protected Placer<BuildingParameters> StablePlacer { get; private set; }
        protected MarketPlacer MarketPlacer { get; private set; }

        protected PlacementDesigner(VillageRoot Root, VillageConfiguration Config, Random Rng)
        {
            this.Root = Root;
            this.Rng = Rng;
            this.Config = Config;
            this.FarmPlacer = new FarmPlacer(Root.Template.Farm.Designs, Root.Template.Windmill.Designs, Rng);
            this.BlacksmithPlacer = new BlacksmithPlacer(Root.Template.Blacksmith.Designs, Rng);
            this.NeighbourhoodPlacer = new NeighbourhoodPlacer(Root.Template.House.Designs, Root.Template.Well.Designs, Rng);
            this.StablePlacer = new Placer<BuildingParameters>(Root.Template.Stable.Designs, Rng);
            this.MarketPlacer = new MarketPlacer(Root.Template.Well.Designs, Rng);
        }

        public abstract PlacementDesign CreateDesign();

        public abstract void FinishPlacements(CollidableStructure Structure, PlacementDesign Design);
    }
}