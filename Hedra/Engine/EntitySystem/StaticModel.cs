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
	        this.Model = ObjectMesh.FromVertexData(BaseMesh);
        }

        public StaticModel(VertexData BaseMesh) : this(null, BaseMesh) { }

        public override void Dispose()
	    {
	        Model.Dispose();
	    }
    }
}
