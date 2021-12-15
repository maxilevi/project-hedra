using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Hedra.Engine.Rendering.Animation;

public struct JointName
{
    private uint _id;
    public string Name { get; }
    
    private static uint _lastId;
    private static readonly object Lock = new ();
    private static readonly Dictionary<string, uint> Map = new ();

    public JointName(string Name)
    {
        this.Name = Name;
        lock (Lock)
        {
            if (Map.TryGetValue(Name, out _id)) return;
            
            Map.TryAdd(Name, _lastId);
            _id = _lastId++;
        }
    }

    public override bool Equals(object? obj)
    {
        return obj is JointName j && _id == j._id;
    }
    
    public static bool operator ==(JointName left, string right)
    {
        return left == new JointName(right);
    }
    
    public static bool operator !=(JointName left, string right)
    {
        return left != new JointName(right);
    }

    public static bool operator ==(JointName left, JointName right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(JointName left, JointName right)
    {
        return !(left == right);
    }
    
    public bool Equals(JointName other)
    {
        return _id == other._id;
    }

    public override int GetHashCode()
    {
        return (int)_id.GetHashCode();
    }
}