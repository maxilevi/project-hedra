/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:22 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
	/// <summary>
	/// Description of SkinningData.
	/// </summary>
	public class SkinningData
	{
		public readonly List<string> JointOrder;
		public readonly List<VertexSkinData> VerticesSkinData;
		
		public SkinningData(List<string> JointOrder, List<VertexSkinData> VerticesSkinData){
			this.JointOrder = JointOrder;
			this.VerticesSkinData = VerticesSkinData;
		}
	}
}
