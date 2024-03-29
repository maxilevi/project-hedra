/*
 * Author: Zaphyk
 * Date: 08/04/2016
 * Time: 11:42 p.m.
 *
 */

using System.Numerics;

namespace Hedra.Engine.Rendering
{
    /// <summary>
    ///     Description of MeshData.
    /// </summary>
    public class MeshData : DataContainer
    {
        public Vector4 TemplateColor;

        public MeshData(Vector4 Color)
        {
            TemplateColor = Color;
            this.Color = new Vector4[] { };
            VerticesArrays = new Vector3[] { };
            Normals = new Vector3[] { };
            HasNormals = true;
        }
    }
}