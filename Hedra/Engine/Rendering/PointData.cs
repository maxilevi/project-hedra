/*
 * Author: Zaphyk
 * Date: 20/03/2016
 * Time: 03:05 p.m.
 *
 */
using System;
using OpenTK;

namespace Hedra.Engine.Rendering
{
    /// <summary>
    /// Constains all the data used for a Geometry Shader
    /// </summary>
    public class PointData : DataContainer
    {
        public PointData(Vector3 Position, Vector4 Color)
        {
            this.VerticesArrays = new Vector3[]{
                Position
            };
            this.Position = Position;
            this.Color = new Vector4[]{Color};
            this.Indices.Add(0);
        }
        
        public void TransformVerts(Vector3 Position){
            for(int i = 0; i < this.VerticesArrays.Length; i++){
                VerticesArrays[i] += Position;
            }
        }
    }
}
