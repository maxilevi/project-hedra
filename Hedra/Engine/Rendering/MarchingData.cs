/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 30/11/2016
 * Time: 01:18 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;

namespace Hedra.Engine.Rendering
{
    /// <summary>
    /// Description of MarchingData.
    /// </summary>
    public class MarchingData : DataContainer
    {
        public Vector4 TemplateColor;
        public MarchingData(Vector4 Color) : base(){
            this.TemplateColor = Color;
        }
    }
}
