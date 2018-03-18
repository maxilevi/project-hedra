/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 15/08/2017
 * Time: 08:51 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of TradeSystem.
	/// </summary>
	public class TradeSystem
	{
		public const int MaxItems = 24;
		public LocalPlayer Player;
		
		public TradeSystem(LocalPlayer Player){
			this.Player = Player;
		}
		
		public void Update(){		
			
		}
		
		public void UpdateMerchandise(){

		}
		
		public void RefreshInventory(){
			
        }

	    public bool Show { get; set; }
	}
}
