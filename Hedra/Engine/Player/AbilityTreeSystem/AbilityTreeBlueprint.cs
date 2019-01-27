/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 06/08/2016
 * Time: 08:05 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Linq;
using Hedra.Engine.ClassSystem.Templates;
using Hedra.Engine.Game;
using Hedra.Engine.SkillSystem;

namespace Hedra.Engine.Player.AbilityTreeSystem
{
    /// <summary>
    /// Description of TreeBlueprint.
    /// </summary>
    public class AbilityTreeBlueprint
    {
        public string Name { get; set; }
        public uint Icon { get; set; }
        public readonly TreeItem[][] Items = new TreeItem[AbilityTree.Columns][];

        public AbilityTreeBlueprint(AbilityTreeTemplate AbilityTreeTemplate)
        {
            Name = AbilityTreeTemplate.Name.ToLowerInvariant();
            for(var i = 0; i < Items.Length; i++)
            {
                Items[i] = new TreeItem[AbilityTree.Rows];
                for(var j = 0; j < Items[i].Length; j++)
                {
                    var skill = AbilityTreeTemplate.Get(i, j);
                    Items[i][j] = skill != null ? new TreeItem
                    {
                        AbilityType = SkillFactory.Instance.Get(skill),
                        Image = GameManager.Player.Toolbar.Skills.First(S => S.GetType() == SkillFactory.Instance.Get(skill)).TextureId,
                    } : new TreeItem();
                }
            }
        }
    }
}
