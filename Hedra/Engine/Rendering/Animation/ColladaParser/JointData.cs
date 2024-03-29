/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:21 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Collections.Generic;
using System.Numerics;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    /// <summary>
    ///     Description of JointData.
    /// </summary>
    public class JointData
    {
        public readonly Matrix4x4 BindLocalTransform;

        public readonly List<JointData> Children = new List<JointData>();
        public readonly int Index;
        public readonly string NameId;

        public JointData(int Index, string NameId, Matrix4x4 BindLocalTransform)
        {
            this.Index = Index;
            this.NameId = NameId;
            this.BindLocalTransform = BindLocalTransform;
        }

        public void AddChild(JointData Child)
        {
            Children.Add(Child);
        }
    }
}