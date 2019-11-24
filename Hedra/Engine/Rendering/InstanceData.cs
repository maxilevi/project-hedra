/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 30/11/2016
 * Time: 05:08 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Hedra.Engine.Core;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Rendering;
using System.Numerics;

namespace Hedra.Engine.Rendering
{
    /// <summary>
    /// Description of InstanceData.
    /// </summary>
    public class InstanceData : LodableObject<InstanceData>, IPositionable, ISearchable
    {
#if DEBUG
        private StackTrace _trace = new StackTrace();
#endif
        private bool _boundsInitialized;
        private Vector3 _bounds;

        public IList<Vector4> Colors { get; set; }
        public IList<float> ExtraData { get; set; }
        public Matrix4x4 TransMatrix { get; set; }
        public object ColorCache { get; set; }
        public object ExtraDataCache { get; set; }
        public bool HasExtraData => (ExtraData != null && ExtraData.Count != 0) || ExtraDataCache != null;
        public bool VariateColor { get; set; } = true;
        public bool GraduateColor { get; set; }
        public bool SkipOnLod { get; set; }
        public bool CanSimplifyProgramatically { get; set; } = true;
        public Func<BlockType, bool> PlaceCondition { get; set; }
        public VertexData OriginalMesh { get; set; }

        public void Apply(Matrix4x4 Transformation)
        {
            TransMatrix *= Transformation;
            ApplyRecursively(I => I.Apply(Transformation));
        }
        
        public Vector3 ApproximateBounds
        {
            get
            {
                if (_boundsInitialized) return _bounds;
                var diagonal = Vector3.Transform(OriginalMesh.SupportPoint(Vector3.One) - OriginalMesh.SupportPoint(-Vector3.One), TransMatrix);
                _bounds.X = diagonal.X;
                _bounds.X = diagonal.X;
                _bounds.X = diagonal.X;
                _boundsInitialized = true;
                return _bounds;
            }
        }
        
        public Vector3 Position => Vector3.Transform(Vector3.Zero, TransMatrix);
    }
}
