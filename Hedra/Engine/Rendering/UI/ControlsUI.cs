/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 10/08/2016
 * Time: 12:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using OpenTK;

namespace Hedra.Engine.Rendering.UI
{
    public class ControlsUI : Panel
    {
        public ControlsUI()
        {
            var help = new Texture(Graphics2D.LoadFromAssets("Assets/UI/Help.png"), Vector2.Zero, Vector2.One);
            AddElement(help);
        }
        
    }
}
