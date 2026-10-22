using System;
using System.Collections.Generic;
using System.Numerics;
using Hedra.Engine.Generation.ChunkSystem;
using System.Threading;
using System.Threading.Tasks;

namespace Hedra.Engine.StructureSystem;

public class OffsetChecker
{
    private readonly Dictionary<Vector2, int> _checkedPositions;
    private readonly Dictionary<Vector2, List<Vector2>> _offsetsToCheckedStructures;
    private readonly object _offsetsToCheckedStructuresLock = new (); 
    private readonly object _checkedPositionsLock = new ();
    private readonly Dictionary<Vector2, (int Min, int Max)> _ranges = new();
    private sealed class PendingPosition
    {
        public readonly int Owner = Thread.CurrentThread.ManagedThreadId;
        public readonly TaskCompletionSource<bool> Completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
    private readonly Dictionary<Vector2, PendingPosition> _pending = new();

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

    public List<Vector2> RegisterRange(Vector2 Original, int Min, int Max)
    {
        var newPositions = new List<Vector2>();
        lock (_checkedPositionsLock)
        {
            lock (_offsetsToCheckedStructuresLock)
            {
                var hasRange = _ranges.TryGetValue(Original, out var previous);
                if (hasRange && Min >= previous.Min && Max <= previous.Max) return newPositions;
                if (!_offsetsToCheckedStructures.TryGetValue(Original, out var positions))
                    _offsetsToCheckedStructures.Add(Original, positions = new List<Vector2>((Max - Min) * (Max - Min)));
                for (var x = Min; x < Max; ++x)
                for (var z = Min; z < Max; ++z)
                {
                    if (hasRange && x >= previous.Min && x < previous.Max && z >= previous.Min && z < previous.Max) continue;
                    var position = Original + new Vector2(x, z) * Chunk.Width;
                    if (_checkedPositions.TryGetValue(position, out var count))
                        _checkedPositions[position] = count + 1;
                    else
                    {
                        _checkedPositions.Add(position, 1);
                        _pending.Add(position, new PendingPosition());
                        newPositions.Add(position);
                    }
                    positions.Add(position);
                }
                _ranges[Original] = hasRange ? (Math.Min(Min, previous.Min), Math.Max(Max, previous.Max)) : (Min, Max);
            }
        }
        return newPositions;
    }

    public void CompletePosition(Vector2 Position, Exception Failure = null)
    {
        PendingPosition pending;
        lock (_checkedPositionsLock) _pending.TryGetValue(Position, out pending);
        if (pending == null) return;
        if (Failure == null) pending.Completion.TrySetResult(true);
        else pending.Completion.TrySetException(Failure);
    }

    public void WaitForRange(Vector2 Original)
    {
        var dependencies = new List<Task>();
        lock (_checkedPositionsLock)
        {
            if (!_offsetsToCheckedStructures.TryGetValue(Original, out var positions)) return;
            foreach (var position in positions)
                if (_pending.TryGetValue(position, out var pending) && !pending.Completion.Task.IsCompletedSuccessfully
                    && (pending.Owner != Thread.CurrentThread.ManagedThreadId || pending.Completion.Task.IsCompleted))
                    dependencies.Add(pending.Completion.Task);
        }
        // The dictionary lock must be released before waiting for other builders.
        foreach (var dependency in dependencies) dependency.GetAwaiter().GetResult();
    }

    public void FailRange(Vector2 Original, Exception Failure)
    {
        lock (_checkedPositionsLock)
        {
            if (!_offsetsToCheckedStructures.TryGetValue(Original, out var positions)) return;
            foreach (var position in positions)
                if (_pending.TryGetValue(position, out var pending) && pending.Owner == Thread.CurrentThread.ManagedThreadId)
                    pending.Completion.TrySetException(Failure);
        }
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
                    _pending.Remove(position);
                    DoIfSuccessful(position);
                }

                _offsetsToCheckedStructures.Remove(ChunkOffset);
                _ranges.Remove(ChunkOffset);
            }
        }
    }
}
