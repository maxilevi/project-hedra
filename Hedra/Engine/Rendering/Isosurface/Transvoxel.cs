using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;

namespace Hedra.Engine.Rendering.Isosurface
{
    public static class Transvoxel
    {
        public const int Primary = 0;
        public const int Secondary = 1;
        private const float S = 1.0f / 256.0f;
        private static readonly Vector3 Unused = new Vector3(1000, 1000, 1000);

        private static byte HiNibble(byte b)
        {
            return (byte)(((b) >> 4) & 0x0F);
        }

        private static byte LoNibble(byte b)
        {
            return (byte)(b & 0x0F);
        }

        private static int Sign(sbyte b)
        {
            return (b >> 7) & 1;
        }

        private static Vector3 Interp(Vector3i v0, Vector3i v1, Vector3i p0, Vector3i p1, VolumeData samples, byte lodIndex = 0)
        {
            return Interp((Vector3)v0, (Vector3)v1, p0, p1, samples, lodIndex);
        }

        private static Vector3 Interp(Vector3 v0, Vector3 v1, Vector3i p0, Vector3i p1, VolumeData samples, byte lodIndex = 0)
        {
            sbyte s0 = samples[p0];
            sbyte s1 = samples[p1];

            int t = (s1 << 8) / (s1 - s0);
            int u = 0x0100 - t;

            if ((t & 0x00ff) == 0)
            {
                // The generated vertex lies at one of the corners so there 
                // is no need to subdivide the interval.
                if (t == 0)
                {
                    return v1;
                }
                return v0;
            }
            else
            {
                for (int i = 0; i < lodIndex; ++i)
                {
                    Vector3 vm = (v0 + v1) / 2;
                    Vector3i pm = (p0 + p1) / 2;

                    sbyte sm = samples[pm];

                    // Determine which of the sub-intervals that contain 
                    // the intersection with the isosurface.
                    if (Sign(s0) != Sign(sm))
                    {
                        v1 = vm;
                        p1 = pm;
                        s1 = sm;
                    }
                    else
                    {
                        v0 = vm;
                        p0 = pm;
                        s0 = sm;
                    }
                }
                t = (s1 << 8) / (s1 - s0);
                u = 0x0100 - t;

                return v0 * t * S + v1 * u * S;
            }
        }

        private static Vector3 ComputeDelta(Vector3 v, int k, int s)
        {
            float p2k = (float)System.Math.Pow(2.0, k); // 1 << k would do the job much more efficient
            float wk = (float)System.Math.Pow(2.0, k - 2.0); // 1 << (k-2)
            Vector3 delta = Vector3.Zero;

            if (k < 1)
            {
                return delta;
            }

            // x
            float p = v.X;
            float p2mk = (float)System.Math.Pow(2.0, -k);
            if (p < p2k)
            {
                // The vertex is inside the minimum cell.
                delta.X = (1.0f - p2mk * p) * wk;
            }
            else if (p > (p2k * (s - 1)))
            {
                // The vertex is inside the maximum cell.
                delta.X = ((p2k * s) - 1.0f - p) * wk;
            }

            // y
            p = v.Y;
            if (p < p2k)
            {
                // The vertex is inside the minimum cell.
                delta.Y = (1.0f - p2mk * p) * wk;
            }
            else if (p > (p2k * (s - 1)))
            {
                // The vertex is inside the maximum cell.
                delta.Y = ((p2k * s) - 1.0f - p) * wk;
            }

            // z
            p = v.Z;
            if (p < p2k)
            {
                // The vertex is inside the minimum cell.
                delta.Z = (1.0f - p2mk * p) * wk;
            }
            else if (p > (p2k * (s - 1)))
            {
                // The vertex is inside the maximum cell.
                delta.Z = ((p2k * s) - 1.0f - p) * wk;
            }

            return delta;
        }

        private static Vector3 ProjectNormal(Vector3 n, Vector3 delta)
        {
            //return Vector3.Cross(n, delta);
            var mat = new Matrix3X3(
                   1.0f - n.X * n.X, -n.X * n.Y, -n.X * n.Z,
                   -n.X * n.Y, 1.0f - n.Y * n.Y, -n.Y * n.Z,
                   -n.X * n.Z, -n.Y * n.Z, 1.0f - n.Z * n.Z);
            return mat * delta;
        }

        private static Vector3i PrevOffset(byte dir)
        {
            return new Vector3i(-(dir & 1),
                                    -((dir >> 1) & 1),
                                    -((dir >> 2) & 1));
        }


        public static int PolygonizeRegularCell(Vector3i min, Vector3 offset, Vector3i xyz,
            VolumeData samples, byte lodIndex, float cellSize,
                                                ref List<Vertex> verts, ref List<int> indices, ref RegularCellCache cache)
        {

            int lodScale = 1 << lodIndex;
            int last = 15 * lodScale;
            byte directionMask = (byte)((xyz.X > 0 ? 1 : 0) | ((xyz.Y > 0 ? 1 : 0) << 1) | ((xyz.Z > 0 ? 1 : 0) << 2));
            byte near = 0;

            // Compute which of the six faces of the block that the vertex 
            // is near. (near is defined as being in boundary cell.)
            for (int i = 0; i < 3; i++)
            {
                //Vertex close to negative face.
                if (min[i] == 0) { near |= (byte)(1 << (i * 2 + 0)); }
                //Vertex close to positive face.
                if (min[i] == last) { near |= (byte)(1 << (i * 2 + 1)); }
            }

            var cornerPositions = new[]
            {
                min + new Vector3i(0, 0, 0) * lodScale,
                min + new Vector3i(1, 0, 0) * lodScale,
                min + new Vector3i(0, 1, 0) * lodScale,
                min + new Vector3i(1, 1, 0) * lodScale,

                min + new Vector3i(0, 0, 1) * lodScale,
                min + new Vector3i(1, 0, 1) * lodScale,
                min + new Vector3i(0, 1, 1) * lodScale,
                min + new Vector3i(1, 1, 1) * lodScale
            };

            Vector3i dif = cornerPositions[7] - cornerPositions[1];

            // Retrieve sample values for all the corners.
            sbyte[] cornerSamples =
                new sbyte[]
                    {
                        samples[cornerPositions[0]],
                        samples[cornerPositions[1]],
                        samples[cornerPositions[2]],
                        samples[cornerPositions[3]],
                        samples[cornerPositions[4]],
                        samples[cornerPositions[5]],
                        samples[cornerPositions[6]],
                        samples[cornerPositions[7]],
                    };

            Vector3[] cornerNormals = new Vector3[8];

            // Determine the index into the edge table which
            // tells us which vertices are inside of the surface
            uint caseCode = (uint)(((cornerSamples[0] >> 7) & 0x01)
                                 | ((cornerSamples[1] >> 6) & 0x02)
                                 | ((cornerSamples[2] >> 5) & 0x04)
                                 | ((cornerSamples[3] >> 4) & 0x08)
                                 | ((cornerSamples[4] >> 3) & 0x10)
                                 | ((cornerSamples[5] >> 2) & 0x20)
                                 | ((cornerSamples[6] >> 1) & 0x40)
                                 | (cornerSamples[7] & 0x80));

            //cache[xyz].CaseIndex = (byte)caseCode;
            if ((caseCode ^ ((cornerSamples[7] >> 7) & 0xff)) == 0)
                return 0;

            // Compute the normals at the cell corners using central difference.
            for (int i = 0; i < 8; ++i)
            {
                var p = cornerPositions[i];
                float nx = (samples[p + Vector3i.UnitX] - samples[p - Vector3i.UnitX]) * 0.5f;
                float ny = (samples[p + Vector3i.UnitY] - samples[p - Vector3i.UnitY]) * 0.5f;
                float nz = (samples[p + Vector3i.UnitZ] - samples[p - Vector3i.UnitZ]) * 0.5f;
                cornerNormals[i] = new Vector3(nx, ny, nz);
                cornerNormals[i].Normalize();
            }

            var c = Tables.RegularCellClass[caseCode];
            var data = Tables.RegularCellData[c];

            byte nt = (byte)data.GetTriangleCount();
            byte nv = (byte)data.GetVertexCount();

            int[] localVertexMapping = new int[12];


            var vert = new Vertex();
            vert.Near = near;
            // Generate all the vertex positions by interpolating along
            // each of the edges that intersect the isosurface.
            for (int i = 0; i < nv; i++)
            {
                ushort edgeCode = Tables.RegularVertexData[caseCode][i];
                byte v0 = HiNibble((byte)(edgeCode & 0xFF));
                byte v1 = LoNibble((byte)(edgeCode & 0xFF));

                Vector3i p0 = cornerPositions[v0];
                Vector3i p1 = cornerPositions[v1];
                Vector3 n0 = cornerNormals[v0];
                Vector3 n1 = cornerNormals[v1];

                int d0 = samples[p0];
                int d1 = samples[p1];

                Debug.Assert(v0 < v1);

                int t = (d1 << 8) / (d1 - d0);
                int u = 0x0100 - t;

                float t0 = t * S;
                float t1 = u * S;

                if ((t & 0x00ff) != 0)
                {
                    // Vertex lies in the interior of the edge.
                    byte dir = HiNibble((byte)(edgeCode >> 8));
                    byte idx = LoNibble((byte)(edgeCode >> 8));
                    bool present = (dir & directionMask) == dir;

                    /*if (present)
                    {
                        var prev = cache[xyz + PrevOffset(dir)];

                        // I don't think this can happen for non-corner vertices.
                        if (prev.CaseIndex == 0 || prev.CaseIndex == 255)
                        {
                            localVertexMapping[i] = -1;
                        }
                        else
                        {
                            localVertexMapping[i] = prev.Verts[idx];
                        }
                    }*/
                    if (!present || localVertexMapping[i] < 0)
                    {
                        localVertexMapping[i] = verts.Count;
                        Vector3 pi = Interp(p0, p1, p0, p1, samples, lodIndex);
                        vert.Primary = offset + pi;
                        vert.Normal = n0 * t0 + n1 * t1;

                        if (near > 0)
                        {
                            Vector3 delta = ComputeDelta(pi, lodIndex, 16);
                            vert.Secondary = vert.Primary + ProjectNormal(vert.Normal, delta);
                        }
                        else
                        {
                            // The vertex is not in a boundary cell, so the 
                            // secondary position will never be used.
                            vert.Secondary = Unused; //vert.Primary;
                        }
                        verts.Add(vert);

                        if ((dir & 8) != 0)
                        {
                            // Store the generated vertex so that other cells can reuse it.
                            //cache[xyz].Verts[idx] = localVertexMapping[i];
                        }
                    }
                }
                else if (t == 0 && v1 == 7)
                {
                    // This cell owns the vertex, so it should be created.
                    localVertexMapping[i] = verts.Count;
                    Vector3 pi = (Vector3)p1 * t0 + (Vector3)p1 * t1;

                    vert.Primary = offset + pi;
                    vert.Normal = n0 * t0 + n1 * t1;

                    if (near > 0)
                    {
                        Vector3 delta = ComputeDelta(pi, lodIndex, 16);
                        vert.Secondary = vert.Primary + ProjectNormal(vert.Normal, delta);
                    }
                    else
                    {
                        // The vertex is not in a boundary cell, so the secondary 
                        // position will never be used.
                        vert.Secondary = Unused;
                    }
                    verts.Add(vert);
                   //cache[xyz].Verts[0] = localVertexMapping[i];
                }
                else
                {
                    // A 3-bit direction code leading to the proper cell can easily be obtained by 
                    // inverting the 3-bit corner index (bitwise, by exclusive ORing with the number 7).
                    // The corner index depends on the value of t, t = 0 means that we're at the higher
                    // numbered endpoint.
                    byte dir = t == 0 ? (byte)(v1 ^ 7) : (byte)(v0 ^ 7);
                    bool present = (dir & directionMask) == dir;

                    if (present)
                    {
                        /*var prev = cache[xyz + PrevOffset(dir)];

                        // The previous cell might not have any geometry, and we 
                        // might therefore have to create a new vertex anyway.
                        if (prev.CaseIndex == 0 || prev.CaseIndex == 255)
                        {
                            localVertexMapping[i] = -1;
                        }
                        else
                        {
                            localVertexMapping[i] = prev.Verts[0];
                        }*/
                    }

                    if (!present || (localVertexMapping[i] < 0))
                    {
                        localVertexMapping[i] = verts.Count;

                        Vector3 pi = (Vector3)p0 * t0 + (Vector3)p1 * t1;
                        vert.Primary = offset + pi;
                        vert.Normal = n0 * t0 + n1 * t1;

                        if (near > 0)
                        {
                            Vector3 delta = ComputeDelta(pi, lodIndex, 16);
                            vert.Secondary = vert.Primary + ProjectNormal(vert.Normal, delta);
                        }
                        else
                        {
                            vert.Secondary = Unused;
                        }
                        verts.Add(vert);
                    }
                }
            }

            for (int t = 0; t < nt; t++)
            {
                for (int i = 0; i < 3; i++)
                {
                    indices.Add(localVertexMapping[data.Indizes()[t * 3 + i]]);
                }
            }

            return nt;
        }

        public static int PolygonizeTransitionCell(Vector3 offset, Vector3i origin,
                                                   Vector3i localX, Vector3i localY, Vector3i localZ,
                                                   int x, int y, float cellSize,
                                                   byte lodIndex, byte axis, byte directionMask,
                                                   VolumeData samples,
                                                   ref IList<Vertex> verts, ref IList<int> indices,
                                                   ref TransitionCellCache cache)
        {
            int lodStep = 1 << lodIndex;
            int sampleStep = 1 << (lodIndex - 1);
            int lodScale = 1 << lodIndex;
            int last = 16 * lodScale;

            byte near = 0;

            // Compute which of the six faces of the block that the vertex 
            // is near. (near is defined as being in boundary cell.)
            for (int i = 0; i < 3; i++)
            {
                // Vertex close to negative face.
                if (origin[i] == 0) { near |= (byte)(1 << (i * 2 + 0)); }
                // Vertex close to positive face.
                if (origin[i] == last) { near |= (byte)(1 << (i * 2 + 1)); }
            }

            Vector3i[] coords = 
                {
                    new Vector3i(0,0,0), new Vector3i(1,0,0), new Vector3i(2,0,0), // High-res lower row
                    new Vector3i(0,1,0), new Vector3i(1,1,0), new Vector3i(2,1,0), // High-res middle row
                    new Vector3i(0,2,0), new Vector3i(1,2,0), new Vector3i(2,2,0), // High-res upper row
                    new Vector3i(0,0,2), new Vector3i(2,0,2), // Low-res lower row
                    new Vector3i(0,2,2), new Vector3i(2,2,2)  // Low-res upper row
                };

            // TODO: Implement Matrix Math
            var mx = localX * sampleStep;
            var my = localY * sampleStep;
            var mz = localZ * sampleStep;

            Matrix3X3 basis = new Matrix3X3((Vector3)mx, (Vector3)my, (Vector3)mz);

            Vector3i[] pos = {
                        origin + basis * coords[0x00], origin + basis * coords[0x01], origin + basis * coords[0x02],
                        origin + basis * coords[0x03], origin + basis * coords[0x04], origin + basis * coords[0x05],
                        origin + basis * coords[0x06], origin + basis * coords[0x07], origin + basis * coords[0x08],
                        origin + basis * coords[0x09], origin + basis * coords[0x0A],
                        origin + basis * coords[0x0B], origin + basis * coords[0x0C]
                };

            Vector3[] normals = new Vector3[13];

            for (int i = 0; i < 9; i++)
            {
                Vector3i p = pos[i];
                float nx = (samples[p + Vector3i.UnitX] - samples[p - Vector3i.UnitX]) * 0.5f;
                float ny = (samples[p + Vector3i.UnitY] - samples[p - Vector3i.UnitY]) * 0.5f;
                float nz = (samples[p + Vector3i.UnitZ] - samples[p - Vector3i.UnitZ]) * 0.5f;
                normals[i] = new Vector3(nx, ny, nz);
                normals[i].Normalize();
            }

            normals[0x9] = normals[0];
            normals[0xA] = normals[2];
            normals[0xB] = normals[6];
            normals[0xC] = normals[8];

            Vector3i[] samplePos = {
                pos[0], pos[1], pos[2], 
                pos[3], pos[4], pos[5], 
                pos[6], pos[7], pos[8],
                pos[0], pos[2],
                pos[6], pos[8]
            };

            uint caseCode = (uint)(Sign(samples[pos[0]]) * 0x001 |
                                   Sign(samples[pos[1]]) * 0x002 |
                                   Sign(samples[pos[2]]) * 0x004 |
                                   Sign(samples[pos[5]]) * 0x008 |
                                   Sign(samples[pos[8]]) * 0x010 |
                                   Sign(samples[pos[7]]) * 0x020 |
                                   Sign(samples[pos[6]]) * 0x040 |
                                   Sign(samples[pos[3]]) * 0x080 |
                                   Sign(samples[pos[4]]) * 0x100);

            if (caseCode == 0 || caseCode == 511)
                return 0;

            cache[x, y].CaseIndex = (byte)caseCode;

            byte classIndex = Tables.TransitionCellClass[caseCode]; // Equivalence class index.
            var data = Tables.TransitionRegularCellData[classIndex & 0x7F];
            bool inverse = (classIndex & 128) != 0;
            int[] localVertexMapping = new int[12];

            int nv = (int)data.GetVertexCount();
            int nt = (int)data.GetTriangleCount();

            Debug.Assert(nv <= 12);
            var vert = new Vertex();

            for (int i = 0; i < nv; i++)
            {
                ushort edgeCode = Tables.TransitionVertexData[caseCode][i];
                byte v0 = HiNibble((byte)edgeCode);
                byte v1 = LoNibble((byte)edgeCode);
                bool lowside = (v0 > 8) && (v1 > 8);

                int d0 = samples[samplePos[v0]];
                int d1 = samples[samplePos[v1]];

                int t = (d1 << 8) / (d1 - d0);
                int u = 0x0100 - t;
                float t0 = t * S;
                float t1 = u * S;

                Vector3 n0 = normals[v0];
                Vector3 n1 = normals[v1];

                vert.Near = near;
                vert.Normal = n0 * t0 + n1 * t1;


                if ((t & 0x00ff) != 0)
                {
                    // Use the reuse information in transitionVertexData
                    byte dir = HiNibble((byte)(edgeCode >> 8));
                    byte idx = LoNibble((byte)(edgeCode >> 8));

                    bool present = (dir & directionMask) == dir;

                    if (present)
                    {
                        // The previous cell is available. Retrieve the cached cell 
                        // from which to retrieve the reused vertex index from.
                        var prev = cache[x - (dir & 1), y - ((dir >> 1) & 1)];

                        if (prev.CaseIndex == 0 || prev.CaseIndex == 511)
                        {
                            // Previous cell does not contain any geometry.
                            localVertexMapping[i] = -1;
                        }
                        else
                        {
                            // Reuse the vertex index from the previous cell.
                            localVertexMapping[i] = prev.Verts[idx];
                        }
                    }
                    if (!present || localVertexMapping[i] < 0)
                    {
                        Vector3 p0 = (Vector3)pos[v0];
                        Vector3 p1 = (Vector3)pos[v1];

                        Vector3 pi = Interp(p0, p1, samplePos[v0], samplePos[v1], samples, lowside ? (byte)lodIndex : (byte)(lodIndex - 1));

                        if (lowside)
                        {
                            switch (axis)
                            {
                                case 0:
                                    pi.X = (float)origin.X;
                                    break;
                                case 1:
                                    pi.Y = (float)origin.Y;
                                    break;
                                case 2:
                                    pi.Z = (float)origin.Z;
                                    break;
                            }

                            Vector3 delta = ComputeDelta(pi, lodIndex, 16);
                            Vector3 proj = ProjectNormal(vert.Normal, delta);

                            vert.Primary = Unused;
                            vert.Secondary = offset + pi + proj;
                        }
                        else
                        {
                            vert.Near = 0;
                            vert.Primary = offset + pi;
                            vert.Secondary = Unused;
                        }

                        localVertexMapping[i] = verts.Count;
                        verts.Add(vert);

                        if ((dir & 8) != 0)
                        {
                            // The vertex can be reused.
                            cache[x, y].Verts[idx] = localVertexMapping[i];
                        }
                    }
                }
                else
                {
                    // Try to reuse corner vertex from a preceding cell.
                    // Use the reuse information in transitionCornerData.
                    byte v = t == 0 ? v1 : v0;
                    byte cornerData = Tables.TransitionCornerData[v];

                    byte dir = HiNibble(cornerData); // High nibble contains direction code.
                    byte idx = LoNibble((cornerData)); // Low nibble contains storage slot for vertex.
                    bool present = (dir & directionMask) == dir;

                    if (present)
                    {
                        // The previous cell is available. Retrieve the cached cell 
                        // from which to retrieve the reused vertex index from.
                        var prev = cache[x - (dir & 1), y - ((dir >> 1) & 1)];

                        if (prev.CaseIndex == 0 || prev.CaseIndex == 511)
                        {
                            // Previous cell does not contain any geometry.
                            localVertexMapping[i] = -1;
                        }
                        else
                        {
                            // Reuse the vertex index from the previous cell.
                            localVertexMapping[i] = prev.Verts[idx];
                        }
                    }

                    if (!present || localVertexMapping[i] < 0)
                    {
                        // A vertex has to be created.
                        Vector3 pi = (Vector3)pos[v];

                        if (v > 8)
                        {
                            // On low-resolution side.
                            // Necessary to translate the intersection point to the 
                            // high-res side so that it is transformed the same way 
                            // as the vertices in the regular cell.
                            switch (axis)
                            {
                                case 0:
                                    pi.X = (float)origin.X;
                                    break;
                                case 1:
                                    pi.Y = (float)origin.Y;
                                    break;
                                case 2:
                                    pi.Z = (float)origin.Z;
                                    break;
                            }

                            Vector3 delta = ComputeDelta(pi, lodIndex, 16);
                            Vector3 proj = ProjectNormal(vert.Normal, delta);

                            vert.Primary = Unused;
                            vert.Secondary = offset + pi + proj;
                        }
                        else
                        {
                            // On high-resolution side.
                            vert.Near = 0; // Vertices on high-res side are never moved.
                            vert.Primary = offset + pi;
                            vert.Secondary = Unused;
                        }
                        localVertexMapping[i] = verts.Count;
                        cache[x, y].Verts[idx] = localVertexMapping[i];
                        verts.Add(vert);
                    }
                }
            }

            for (long t = 0; t < nt; ++t)
            {
                if (inverse)
                {
                    indices.Add(localVertexMapping[data.Indizes()[t * 3 + 2]]);
                    indices.Add(localVertexMapping[data.Indizes()[t * 3 + 1]]);
                    indices.Add(localVertexMapping[data.Indizes()[t * 3 + 0]]);
                    //indices.push_back(localVertexMapping[ptr[2]]);
                    //indices.push_back(localVertexMapping[ptr[1]]);
                    //indices.push_back(localVertexMapping[ptr[0]]);
                }
                else
                {
                    indices.Add(localVertexMapping[data.Indizes()[t * 3 + 0]]);
                    indices.Add(localVertexMapping[data.Indizes()[t * 3 + 1]]);
                    indices.Add(localVertexMapping[data.Indizes()[t * 3 + 2]]);
                    // indices.push_back(localVertexMapping[ptr[0]]);
                    // indices.push_back(localVertexMapping[ptr[1]]);
                    // indices.push_back(localVertexMapping[ptr[2]]);
                }
            }

            return nt;
        }
    }
    
    public struct Vertex
    {
        public Vector3 Primary;
        public Vector3 Secondary;
        public Vector3 Normal;
        public byte Near;
    }
}