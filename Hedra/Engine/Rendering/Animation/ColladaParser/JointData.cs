/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:21 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    /// <summary>
    /// Description of JointData.
    /// </summary>
    public class JointData
    {
        public readonly int Index;
        public readonly string NameId;
        public readonly Matrix4 BindLocalTransform;
        
        public readonly List<JointData> Children = new List<JointData>();
        
        public JointData(int Index, string NameId, Matrix4 BindLocalTransform){
            this.Index = Index;
            this.NameId = NameId;
            this.BindLocalTransform = BindLocalTransform;
        }
        
        public void AddChild(JointData Child){
            Children.Add(Child);
        }
    }
}
