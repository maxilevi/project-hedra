/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 28/11/2016
 * Time: 06:51 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using Hedra.Engine.Rendering;
using Hedra.Engine.Generation;

namespace Hedra.Engine.EntitySystem
{
	/// <summary>
	/// Description of StaticModel.
	/// </summary>
	public class StaticModel : Model
	{
		public ObjectMesh Mesh;
		public Entity Parent;
		public override Vector3 TargetRotation {get; set;}
		
		public StaticModel(){}
		public StaticModel(Entity Parent, VertexData BaseMesh) : base(){
			this.Mesh = ObjectMesh.FromVertexData(BaseMesh);
			this.Parent = Parent;
		}
		public override void Update(){}
		public override void Run(){}
		public override void Idle(){}
	}
}
