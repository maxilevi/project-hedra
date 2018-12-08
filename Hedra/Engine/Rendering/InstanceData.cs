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
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.Rendering
{
    /// <summary>
    /// Description of InstanceData.
    /// </summary>
    public class InstanceData : IPositionable
    {
        private bool _boundsInitialized;
        private Vector3 _bounds;
        private Dictionary<int, InstanceData> _lodVersions;
        private VertexData _originalMesh;
        
        public List<Vector4> Colors { get; set; }
        public List<float> ExtraData { get; set; }
        public Matrix4 TransMatrix { get; set; }
        public Vector4 ColorCache { get; set; } = -Vector4.One;
        public float ExtraDataCache { get; set; } = -1;
        public bool HasExtraData { get; set; } = true;
        public bool VariateColor { get; set; } = true;
        public bool GraduateColor { get; set; }
        public Func<BlockType, bool> PlaceCondition { get; set; }

        public VertexData OriginalMesh
        {
            get => _originalMesh;
            set
            {
                _originalMesh = value;
                //UpdateLODs();
            }
        }
        
        /* ReSharper disable once InconsistentNaming */
        public void AddLOD(InstanceData Model, int Level)
        {
            if(Level != 2 && Level != 4 && Level != 8)
                throw new ArgumentOutOfRangeException($"LOD needs to be either 2, 4 or 8, '{Level}' given.");

            if (_lodVersions == null) _lodVersions = new Dictionary<int, InstanceData>
            {
                {2, null},
                {4, null},
                {8, null}
            };

            _lodVersions[Level] = Model;
        }

        public InstanceData Get(int Lod)
        {
            if (Lod == 1 || _lodVersions == null) return this;
            var selectedLod = _lodVersions[Lod];
            while (selectedLod == null)
            {
                selectedLod = Lod > 1
                    ? _lodVersions[Lod = Lod / 2]
                    : this;
            }
            return selectedLod;
        }

        public void Apply(Matrix4 Transformation)
        {
            TransMatrix *= Transformation;
            if(_lodVersions == null) return;
            if(_lodVersions[2] != null) _lodVersions[2].TransMatrix *= Transformation;
            if(_lodVersions[4] != null) _lodVersions[4].TransMatrix *= Transformation;
            if(_lodVersions[8] != null) _lodVersions[8].TransMatrix *= Transformation;
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
