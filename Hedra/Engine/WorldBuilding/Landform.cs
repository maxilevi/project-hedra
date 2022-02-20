using System;
using System.Numerics;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Numerics;

namespace Hedra.Engine.WorldBuilding;

public class Landform
{
    private readonly Vector2 _position;
    private readonly float _width;
    private readonly float _height;
    private readonly float[][] _data;
    
    public Landform(Vector2 Position, float[][] Data)
    {
        _position = Position;
        _data = Data;
        _width = Data.Length;
        _height = Data[0].Length;
    }

    public float Apply(Vector2 Position)
    {
        if (!HasPoint(Position, out var delta)) return 0;
        return _data[(int)delta.X][(int)delta.Y] * 96;
    }


    public bool HasPoint(Vector2 Position, out Vector2 Delta)
    {
        Delta = (Position - _position - new Vector2(_width, _height) * 0.5f);
        if (Delta.X < 0 || Delta.Y < 0 || Delta.X >= _width || Delta.Y >= _height)
            return false;
        return true;
    }
}