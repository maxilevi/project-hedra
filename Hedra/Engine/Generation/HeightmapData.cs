/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 21/07/2016
 * Time: 02:51 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Hedra.Engine.IO;

namespace Hedra.Engine.Generation
{
    /// <summary>
    ///     Description of HeightmapData.
    /// </summary>
    public class HeightmapData
    {
        public const float Version = 1.3f;
        public float[][] Heightmap;
        public BlockType[][] HeightmapType;

        public byte[] ToByteArray()
        {
            using (var Ms = new MemoryStream())
            {
                using (var Bw = new BinaryWriter(Ms))
                {
                    if (Heightmap == null)
                    {
                        Heightmap = new float[1][];
                        Heightmap[0] = new float[0];
                        HeightmapType = new BlockType[1][];
                        HeightmapType[0] = new BlockType[0];
                    }

                    Bw.Write(Version);
                    Bw.Write(Heightmap.Length);
                    Bw.Write(Heightmap[0].Length);
                    for (var x = 0; x < Heightmap.Length; x++)
                    for (var y = 0; y < Heightmap[0].Length; y++)
                        Bw.Write(Heightmap[x][y]);

                    Bw.Write(HeightmapType.Length);
                    Bw.Write(HeightmapType[0].Length);
                    for (var x = 0; x < HeightmapType.Length; x++)
                    for (var y = 0; y < HeightmapType[0].Length; y++)
                        Bw.Write((byte)HeightmapType[x][y]);
                }

                return Ms.ToArray();
            }
        }

        public HeightmapData FromByteArray(byte[] arrBytes)
        {
            using (var Ms = new MemoryStream(arrBytes))
            {
                using (var Br = new BinaryReader(Ms))
                {
                    var CurrentVersion = Br.ReadSingle();
                    if (CurrentVersion < 1.3f)
                    {
                        Log.WriteLine("Old Map Format -> Converting!");

                        using (var Ms2 = new MemoryStream(arrBytes))
                        {
                            var binForm = new BinaryFormatter();
                            var obj = (HeightmapData)binForm.Deserialize(Ms2);
                            Heightmap = obj.Heightmap;
                            HeightmapType = obj.HeightmapType;
                            return this;
                        }
                    }

                    Heightmap = new float[Br.ReadInt32()][];
                    var SizeY = Br.ReadInt32();

                    for (var x = 0; x < Heightmap.Length; x++)
                    {
                        Heightmap[x] = new float[SizeY];
                        for (var y = 0; y < Heightmap[0].Length; y++) Heightmap[x][y] = Br.ReadSingle();
                    }

                    HeightmapType = new BlockType[Br.ReadInt32()][];
                    SizeY = Br.ReadInt32();

                    for (var x = 0; x < HeightmapType.Length; x++)
                    {
                        HeightmapType[x] = new BlockType[SizeY];
                        for (var y = 0; y < HeightmapType[0].Length; y++)
                            HeightmapType[x][y] = (BlockType)Br.ReadByte();
                    }
                }
            }

            return this;
        }
    }
}