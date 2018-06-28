/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 06/08/2016
 * Time: 08:05 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using OpenTK;

namespace Hedra.Engine.Player.Skills
{
	/// <summary>
	/// Description of TreeBlueprint.
	/// </summary>
	internal abstract class AbilityTreeBlueprint
	{
		public TreeItem[][] Items = new TreeItem[3][];
		public Vector4 ActiveColor;

	    protected AbilityTreeBlueprint(){
			for(var i = 0; i < Items.Length; i++){
				Items[i] = new TreeItem[5];
				for(var j = 0; j < Items[i].Length; j++){
					Items[i][j] = new TreeItem();
				}
			}
		}
	}
}
