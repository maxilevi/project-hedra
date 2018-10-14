using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using OpenTK;

namespace Hedra.Engine.WorldBuilding
{
	/// <summary>
	/// Description of Campfire.
	/// </summary>
	public class TravellingMerchant : Campfire
	{
		public TravellingMerchant(Vector3 Position) : base(Position)
		{
		    Executer.ExecuteOnMainThread(
                () => World.WorldBuilding.SpawnHumanoid(HumanType.TravellingMerchant, Position + Vector3.UnitX * -12f)
            );
        }
	}
}
