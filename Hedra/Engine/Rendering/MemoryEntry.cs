/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 13/10/2017
 * Time: 06:20 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace Hedra.Engine.Rendering
{
    /// <summary>
    ///     Description of MemoryEntry.
    /// </summary>
    public class MemoryEntry
    {
        public int Offset, Length;

        public static int Compare(MemoryEntry Entry1, MemoryEntry Entry2)
        {
            if (Entry1.Offset > Entry2.Offset)
                return 1;
            if (Entry1.Offset < Entry2.Offset)
                return -1;
            return 0;
        }
    }
}