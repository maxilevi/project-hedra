/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 09/10/2017
 * Time: 06:36 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace Hedra.Engine.Rendering
{
    /// <summary>
    ///     Description of ChunkRenderCommand.
    /// </summary>
    public class ChunkRenderCommand
    {
        public int DrawCount, VertexCount;
        public MemoryEntry[] Entries;
        public int ByteOffset => Entries[0].Offset;
    }
}