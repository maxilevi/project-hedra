using System;
using System.Collections.Generic;
using System.Numerics;

namespace Hedra.Engine.StructureSystem;

public class OffsetChecker
{
    private readonly Dictionary<Vector2, int> _checkedPositions;
    private readonly Dictionary<Vector2, List<Vector2>> _offsetsToCheckedStructures;
    private readonly object _offsetsToCheckedStructuresLock = new (); 
    private readonly object _checkedPositionsLock = new ();

    public OffsetChecker()
    {
        _offsetsToCheckedStructures = new Dictionary<Vector2, List<Vector2>>();
        _checkedPositions = new Dictionary<Vector2, int>();
    }
    
    public bool RegisterOffset(Vector2 Original, Vector2 Checked)
    {
        var skip = false;
        lock (_checkedPositionsLock)
        {
            lock (_offsetsToCheckedStructuresLock)
            {
                if (!_checkedPositions.ContainsKey(Checked))
                {
                    _checkedPositions.Add(Checked, 1);
                }
                else
                {
                    _checkedPositions[Checked] += 1;
                    skip = true;
                }
                if (!_offsetsToCheckedStructures.ContainsKey(Original))
                    _offsetsToCheckedStructures.Add(Original, new List<Vector2>());
                _offsetsToCheckedStructures[Original].Add(Checked);
            }
        }

        return skip;
    }
    
    public void DeleteOffset(Vector2 ChunkOffset, Action<Vector2> DoIfSuccessful)
    {
        lock (_checkedPositionsLock)
        {
            lock (_offsetsToCheckedStructuresLock)
            {
                if (!_offsetsToCheckedStructures.ContainsKey(ChunkOffset)) return;
                foreach (var position in _offsetsToCheckedStructures[ChunkOffset])
                {
                    _checkedPositions[position] -= 1;
                    if (_checkedPositions[position] != 0) continue;
                    
                    _checkedPositions.Remove(position);
                    DoIfSuccessful(position);
                }

                _offsetsToCheckedStructures.Remove(ChunkOffset);
            }
        }
    }
}