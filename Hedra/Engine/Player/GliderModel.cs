/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 28/07/2016
 * Time: 05:22 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.Player
{

	public class GliderModel : Model
	{
		public EntityMesh BaseMesh;
		
		public GliderModel(){
			BaseMesh = EntityMesh.FromVertexData(AssetManager.PlyLoader("Assets/Items/Glider.ply", Vector3.One, Vector3.Zero, Vector3.Zero));
		}
		
		public override void Idle(){}
		public override void Run(){}
	}
}
