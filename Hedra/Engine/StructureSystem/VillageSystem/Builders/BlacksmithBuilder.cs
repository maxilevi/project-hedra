using Hedra.Engine.Generation;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
	internal class BlacksmithBuilder : Builder<BlacksmithParameters>
	{
		public override BuildingOutput Paint(BlacksmithParameters Parameters, BuildingOutput Input)
		{
			Input.Model.GraduateColor(Vector3.UnitY);
			return base.Paint(Parameters, Input);
		}

		public override BuildingOutput Build(BlacksmithParameters Parameters, VillageCache Cache)
		{
			World.AddStructure(new LampPost(Parameters.Position + Parameters.Design.Oven)
			{
				Radius = 32,
				LightColor = new Vector3(1f, .5f, 0f)
			});
			return base.Build(Parameters, Cache);
		}

		public override void BuildNPCs(BlacksmithParameters Parameters)
		{
			World.QuestManager.SpawnHumanoid(HumanType.Blacksmith, Parameters.Position + Parameters.Design.Blacksmith);
		}
	}
}