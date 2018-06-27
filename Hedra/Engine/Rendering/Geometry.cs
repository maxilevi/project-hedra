using System;
using System.Linq;
using OpenTK;

namespace Hedra.Engine.Rendering
{
    internal static class Geometry
    {
        public static GeometryData Cube()
        {
            return new GeometryData
            {
                Vertices = BoxVertices,
            };
        }

        public static GeometryData UVSphere(int Segments)
        {
            Vector3[] vertices = new Vector3[Segments * (Segments - 1) + 2];
            Vector2[] uvs = new Vector2[Segments * (Segments - 1) + 2];
            ushort[] elements = new ushort[2 * Segments * (Segments - 1) * 3];

            double deltaLatitude = Math.PI / Segments;
            double deltaLongitude = Math.PI * 2.0 / Segments;
            int index = 0;

            // create the rings of the dome using polar coordinates
            for (int i = 1; i < Segments; i++)
            {
                double r0 = Math.Sin(i * deltaLatitude);
                double y0 = Math.Cos(i * deltaLatitude);

                for (int j = 0; j < Segments; j++)
                {
                    double x0 = r0 * Math.Sin(j * deltaLongitude);
                    double z0 = r0 * Math.Cos(j * deltaLongitude);

                    vertices[index] = new Vector3((float)x0, (float)y0, (float)z0);
                    uvs[index++] = new Vector2(0, 1.0f - (float)y0);
                }
            }

            // create the top of the dome
            vertices[index] = new Vector3(0, 1, 0);
            uvs[index++] = new Vector2(0, 0);

            // create the bottom of the dome
            vertices[index] = new Vector3(0, -1, 0);
            uvs[index] = new Vector2(0, 2);

            // create the faces of the rings
            index = 0;
            for (int i = 0; i < Segments - 2; i++)
            {
                for (int j = 0; j < Segments; j++)
                {
                    elements[index++] = (ushort)(Segments * i + j);
                    elements[index++] = (ushort)(Segments * i + (j + 1) % Segments);
                    elements[index++] = (ushort)(Segments * (i + 1) + (j + 1) % Segments);
                    elements[index++] = (ushort)(Segments * i + j);
                    elements[index++] = (ushort)(Segments * (i + 1) + (j + 1) % Segments);
                    elements[index++] = (ushort)(Segments * (i + 1) + j);
                }
            }

            // create the faces of the top of the dome
            for (int i = 0; i < Segments; i++)
            {
                elements[index++] = (ushort)(Segments * (Segments - 1));
                elements[index++] = (ushort)((i + 1) % Segments);
                elements[index++] = (ushort)i;
            }

            // create the faces of the bottom of the dome
            for (int i = 0; i < Segments; i++)
            {
                elements[index++] = (ushort)(Segments * (Segments - 1) + 1);
                elements[index++] = (ushort)(Segments * (Segments - 2) + i);
                elements[index++] = (ushort)(Segments * (Segments - 2) + (i + 1) % Segments);
            }

            return new GeometryData
            {
                Vertices = vertices,
                Indices = elements,
                Normals = CalculateNormals(vertices, elements),
                UVs = uvs
            };
        }

        private static Vector3[] CalculateNormals(Vector3[] vertexData, ushort[] elementData)
        {
            Vector3 b1, b2, normal;
            Vector3[] normalData = new Vector3[vertexData.Length];

            for (int i = 0; i < elementData.Length / 3; i++)
            {
                int cornerA = elementData[i * 3];
                int cornerB = elementData[i * 3 + 1];
                int cornerC = elementData[i * 3 + 2];

                b1 = vertexData[cornerB] - vertexData[cornerA];
                b2 = vertexData[cornerC] - vertexData[cornerA];

                normal = Mathf.CrossProduct(b1, b2).Normalized();

                normalData[cornerA] += normal;
                normalData[cornerB] += normal;
                normalData[cornerC] += normal;
            }

            for (int i = 0; i < normalData.Length; i++) normalData[i] = normalData[i].Normalized();

            return normalData;
        }

        private static readonly Vector3[] BoxVertices = 
        {
            new Vector3(-1.0f,  1.0f, -1.0f),
            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, -1.0f, -1.0f),
            new Vector3(1.0f,  1.0f, -1.0f),
            new Vector3(-1.0f,  1.0f, -1.0f),

            new Vector3(-1.0f, -1.0f,  1.0f),
            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3(-1.0f,  1.0f, -1.0f),
            new Vector3(-1.0f,  1.0f, -1.0f),
            new Vector3(-1.0f,  1.0f,  1.0f),
            new Vector3(-1.0f, -1.0f,  1.0f),

            new Vector3(1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, -1.0f,  1.0f),
            new Vector3(1.0f,  1.0f,  1.0f),
            new Vector3(1.0f,  1.0f,  1.0f),
            new Vector3(1.0f,  1.0f, -1.0f),
            new Vector3(1.0f, -1.0f, -1.0f),

            new Vector3(-1.0f, -1.0f,  1.0f),
            new Vector3(-1.0f,  1.0f,  1.0f),
            new Vector3(1.0f,  1.0f,  1.0f),
            new Vector3(1.0f,  1.0f,  1.0f),
            new Vector3(1.0f, -1.0f,  1.0f),
            new Vector3(-1.0f, -1.0f,  1.0f),

            new Vector3(-1.0f,  1.0f, -1.0f),
            new Vector3(1.0f,  1.0f, -1.0f),
            new Vector3(1.0f,  1.0f,  1.0f),
            new Vector3(1.0f,  1.0f,  1.0f),
            new Vector3(-1.0f,  1.0f,  1.0f),
            new Vector3(-1.0f,  1.0f, -1.0f),

            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3(-1.0f, -1.0f,  1.0f),
            new Vector3(1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, -1.0f, -1.0f),
            new Vector3(-1.0f, -1.0f,  1.0f),
            new Vector3(1.0f, -1.0f,  1.0f)
        };
    }
}
