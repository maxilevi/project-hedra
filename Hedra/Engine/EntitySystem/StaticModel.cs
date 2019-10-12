/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 28/11/2016
 * Time: 06:51 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Rendering;
using OpenToolkit.Mathematics;

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
            var model = ObjectMesh.FromVertexData(Mesh);
            if (Model != null)
            {
                model.Position = this.Position;
                model.LocalRotation = this.LocalRotation;
                model.LocalRotationPoint = this.LocalRotationPoint;
                model.Rotation = this.Rotation;
                model.LocalPosition = this.LocalPosition;
                model.BeforeRotation = this.BeforeRotation;
                model.TransformationMatrix = this.TransformationMatrix;
                model.Scale = this.Scale;
                model.Alpha = this.Alpha;
                model.ApplyFog = this.ApplyFog;
                model.Outline = this.Outline;
                model.Pause = this.Pause;
                model.Enabled = this.Enabled;
                Model.Dispose();
            }
            this.Model = model;
        }
        
        public override void Dispose()
        {
            Model.Dispose();
        }
    }
}
