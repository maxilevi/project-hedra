/*
 * Author: Zaphyk
 * Date: 08/04/2016
 * Time: 11:42 p.m.
 *
 */
using System;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.Rendering
{
    /// <summary>
    /// Description of MeshData.
    /// </summary>
    public class MeshData : DataContainer
    {
        public Vector4 TemplateColor;
        public MeshData(Vector4 Color)
        {
            this.TemplateColor = Color;
            this.Color = new Vector4[]{};
            this.VerticesArrays = new Vector3[]{ };
            this.Normals = new Vector3[]{ };
            this.HasNormals = true;
        }
    }
}
