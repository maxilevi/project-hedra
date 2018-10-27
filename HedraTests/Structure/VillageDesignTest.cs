using System;
using Hedra.Engine.Generation;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using NUnit.Framework;
using OpenTK;

namespace HedraTests.Structure
{
    /*[TestFixture]
    public class VillageDesignTest : DesignTest<VillageDesign>
    {
        protected override CollidableStructure CreateStructure()
        {
            var structure = base.CreateStructure();
            var builder = new VillageBuilder(VillageRoot.FromTemplate(new VillageTemplate
            {
                Name = string.Empty,
                House = new HouseTemplate
                {
                    Designs = new DesignTemplate[1]
                    {
                        new DesignTemplate()
                    }
                },
                Well = new WellTemplate
                {
                    Designs = new DesignTemplate[1]
                    {
                        new DesignTemplate()
                    }
                    
                },
                Stable = new StableTemplate
                {
                    Designs = new DesignTemplate[1]
                    {
                        new DesignTemplate()
                    }
                },
                Blacksmith = new BlacksmithTemplate
                {
                    Designs = new BlacksmithDesignTemplate[1]
                    {
                        new BlacksmithDesignTemplate()
                    }
                },
                Farm = new FarmTemplate
                {
                    Designs = new DesignTemplate[1]
                    {
                        new DesignTemplate()
                    }
                },
                Windmill = new WindmillTemplate
                {
                    Designs = new DesignTemplate[1]
                    {
                        new DesignTemplate()
                    }
                },
            }), new Random());
            var design = builder.DesignVillage();
            design.Translate(structure.Position);
            builder.PlaceGroundwork(design);

            structure.Parameters.Set("Builder", builder);
            structure.Parameters.Set("Design", design);
            return structure;
        }
    }*/
}