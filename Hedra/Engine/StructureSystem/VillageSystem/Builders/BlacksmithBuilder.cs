using System;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
	public class BlacksmithBuilder : Builder<BlacksmithParameters>
	{
	    public override bool Place(BlacksmithParameters Parameters, VillageCache Cache)
	    {
	        return this.PlaceGroundwork(Parameters.Position, this.ModelRadius(Parameters, Cache) * .75f, BlockType.Path);
        }

		public override BuildingOutput Paint(BlacksmithParameters Parameters, BuildingOutput Input)
		{
			for(var i = 0; i < Input.Models.Count; i++)
				Input.Models[i].GraduateColor(Vector3.UnitY);
			return base.Paint(Parameters, Input);
		}

		public override BuildingOutput Build(BlacksmithParameters Parameters, VillageCache Cache, Random Rng, Vector3 Center)
		{
			World.AddStructure(new LampPost(Parameters.Position + Parameters.Design.Oven * Parameters.Design.Scale)
			{
				Radius = 32,
				LightColor = new Vector3(1f, .5f, 0f)
			});
			return base.Build(Parameters, Cache, Rng, Center);
		}

		public override void Polish(BlacksmithParameters Parameters)
		{
			World.WorldBuilding.SpawnHumanoid(HumanType.Blacksmith, Parameters.Position + Parameters.Design.Blacksmith * Parameters.Design.Scale);
		}
	}
}