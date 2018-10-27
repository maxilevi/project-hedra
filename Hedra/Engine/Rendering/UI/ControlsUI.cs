/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 10/08/2016
 * Time: 12:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using OpenTK;
using Hedra.Engine.Rendering;
using Hedra.Engine.Management;
using System.Collections.Generic;

namespace Hedra.Engine.Rendering.UI
{
    /// <summary>
    /// Description of ControlUI.
    /// </summary>
    public class ControlsUi
    {
        public Vector2 TargetResolution = new Vector2(1366,768);
        public List<UIElement> ControlsElements = new List<UIElement>();
        
        public ControlsUi(Color C){
            Color fontColor = C;
            Texture help = new Texture(Graphics2D.LoadFromAssets("Assets/UI/Help.png"), Vector2.Zero, Vector2.One);
            ControlsElements.Add(help);
        }
    }
}
