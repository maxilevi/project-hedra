/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 28/07/2016
 * Time: 05:22 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Engine.EntitySystem;
using OpenTK;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Rendering;

namespace Hedra.Engine.Player
{

    public class GliderModel : UpdatableObjectMeshModel
    {
        public GliderModel() : base(null)
        {
            Model = ObjectMesh.FromVertexData(
                AssetManager.PLYLoader("Assets/Items/Misc/Glider.ply", Vector3.One * 2.0f)
                );
        }
    }
}
