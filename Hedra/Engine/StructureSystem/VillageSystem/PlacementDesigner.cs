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
        protected VillageRoot Root { get; }
        protected Random Rng { get; }
        protected VillageConfiguration Config { get; }
        protected FarmPlacer FarmPlacer { get; }
        protected BlacksmithPlacer BlacksmithPlacer { get; }
        protected HousePlacer HousePlacer { get; }
        protected Placer<BuildingParameters> StablePlacer { get; }
        protected MarketPlacer MarketPlacer { get; }
        protected GenericPlacer InnPlacer { get; }
        
        protected PlacementDesigner(VillageRoot Root, VillageConfiguration Config, Random Rng)
        {
            this.Root = Root;
            this.Rng = Rng;
            this.Config = Config;
            FarmPlacer = new FarmPlacer(Root.Template.Farm, Root.Template.Farm.Designs, Root.Template.Windmill.Designs, Rng);
            BlacksmithPlacer = new BlacksmithPlacer(Root.Template.Blacksmith.Designs, Rng);
            HousePlacer = new HousePlacer(Root.Template.House.Designs, Root.Template.Well.Designs, Rng);
            StablePlacer = new Placer<BuildingParameters>(Root.Template.Stable.Designs, Rng);
            MarketPlacer = new MarketPlacer(Root.Template.Well.Designs, Rng);
            InnPlacer = new GenericPlacer(Root.Template.Inn.Designs, Rng, 1);
        }

        public abstract PlacementDesign CreateDesign();

        public abstract void FinishPlacements(CollidableStructure Structure, PlacementDesign Design);
    }
}