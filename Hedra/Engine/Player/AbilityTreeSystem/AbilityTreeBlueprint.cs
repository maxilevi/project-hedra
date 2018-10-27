/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 06/08/2016
 * Time: 08:05 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Engine.Player.Skills;

namespace Hedra.Engine.Player.AbilityTreeSystem
{
	/// <summary>
	/// Description of TreeBlueprint.
	/// </summary>
	public abstract class AbilityTreeBlueprint
	{
		public readonly TreeItem[][] Items = new TreeItem[AbilityTree.Layers][];

	    protected AbilityTreeBlueprint()
	    {
			for(var i = 0; i < Items.Length; i++)
			{
				Items[i] = new TreeItem[AbilityTree.AbilityCount / AbilityTree.Layers];
				for(var j = 0; j < Items[i].Length; j++)
				{
					Items[i][j] = new TreeItem();
				}
			}
		}
	}
}
