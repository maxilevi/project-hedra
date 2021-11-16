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

namespace Hedra.Engine.EntitySystem
{
    /// <summary>
    ///     Description of StaticModel.
    /// </summary>
    public class StaticModel : UpdatableObjectMeshModel
    {
        public StaticModel(IEntity Parent, VertexData BaseMesh) : base(Parent)
        {
            SetModel(BaseMesh);
        }

        public StaticModel(VertexData BaseMesh) : this(null, BaseMesh)
        {
        }

        public override bool IsStatic => true;

        public void SetModel(VertexData Mesh)
        {
            var model = ObjectMesh.FromVertexData(Mesh);
            if (Model != null)
            {
                model.Position = Position;
                model.LocalRotation = LocalRotation;
                model.LocalRotationPoint = LocalRotationPoint;
                model.Rotation = Rotation;
                model.LocalPosition = LocalPosition;
                model.BeforeRotation = BeforeRotation;
                model.TransformationMatrix = TransformationMatrix;
                model.Scale = Scale;
                model.Alpha = Alpha;
                model.ApplyFog = ApplyFog;
                model.Outline = Outline;
                model.Pause = Pause;
                model.Enabled = Enabled;
                Model.Dispose();
            }

            Model = model;
        }

        public override void Dispose()
        {
            Model.Dispose();
        }
    }
}