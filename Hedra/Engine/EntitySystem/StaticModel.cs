/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 28/11/2016
 * Time: 06:51 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.EntitySystem
{
	/// <summary>
	/// Description of StaticModel.
	/// </summary>
	public class StaticModel : UpdatableObjectMeshModel
	{
	    public override bool IsStatic => true;

	    public StaticModel(IEntity Parent, VertexData BaseMesh) : base(Parent)
	    {
	        this.SetModel(BaseMesh);
        }

        public StaticModel(VertexData BaseMesh) : this(null, BaseMesh) { }

		public void SetModel(VertexData Mesh)
		{
			this.Model = ObjectMesh.FromVertexData(Mesh);
			this.Model.Position = this.Position;
			this.Model.Rotation = this.Rotation;
			this.Model.RotationPoint = this.RotationPoint;
			this.Model.LocalRotation = this.LocalRotation;
			this.Model.LocalPosition = this.LocalPosition;
			this.Model.BeforeLocalRotation = this.BeforeLocalRotation;
			this.Model.TransformationMatrix = this.TransformationMatrix;
			this.Model.TargetRotation = this.TargetRotation;
			this.Model.TargetPosition = this.TargetPosition;
			this.Model.Scale = this.Scale;
			this.Model.Alpha = this.Alpha;
			this.Model.ApplyFog = this.ApplyFog;
			this.Model.Outline = this.Outline;
			this.Model.Pause = this.Pause;
		}
		
        public override void Dispose()
	    {
	        Model.Dispose();
	    }
    }
}
