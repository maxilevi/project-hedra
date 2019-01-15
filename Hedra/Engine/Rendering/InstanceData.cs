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
using Hedra.Engine.Core;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.Rendering
{
    /// <summary>
    /// Description of InstanceData.
    /// </summary>
    public class InstanceData : LodableObject<InstanceData>, IPositionable, ISearchable
    {
        private bool _boundsInitialized;
        private Vector3 _bounds;

        public List<Vector4> Colors { get; set; }
        public List<float> ExtraData { get; set; }
        public Matrix4 TransMatrix { get; set; }
        public Vector4 ColorCache { get; set; } = -Vector4.One;
        public float ExtraDataCache { get; set; } = -1;
        public bool HasExtraData { get; set; } = true;
        public bool VariateColor { get; set; } = true;
        public bool GraduateColor { get; set; }
        public bool SkipOnLod { get; set; }
        public Func<BlockType, bool> PlaceCondition { get; set; }
        public VertexData OriginalMesh { get; set; }


        public void Apply(Matrix4 Transformation)
        {
            TransMatrix *= Transformation;
            ApplyRecursively(I => I.Apply(Transformation));
        }
        
        public Vector3 Bounds
        {
            get
            {
                if (_boundsInitialized) return _bounds;
                var data = OriginalMesh.Clone();
                data.Transform(TransMatrix);
                _bounds.X = data.SupportPoint(Vector3.UnitX).X - data.SupportPoint(-Vector3.UnitX).X;
                _bounds.Y = data.SupportPoint(Vector3.UnitY).Y - data.SupportPoint(-Vector3.UnitY).Y;
                _bounds.Z = data.SupportPoint(Vector3.UnitZ).Z - data.SupportPoint(-Vector3.UnitZ).Z;
                _boundsInitialized = true;
                data.Dispose();
                return _bounds;
            }
        }
        
        public Vector3 Position => Vector3.TransformPosition(Vector3.Zero, TransMatrix);
    }
}
