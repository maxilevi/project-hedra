using System;
using System.Runtime.CompilerServices;

namespace Hedra.Framework;

public unsafe struct Pointer
{
#if DEBUG
    private readonly bool _initialized;
#endif
    private readonly int _offset;
    private readonly Func<IntPtr> _base;
    private void* _cache;
    private IntPtr _cachedBase;
    
    public Pointer(Func<IntPtr> Base, int Offset)
    {
#if DEBUG
        _initialized = true;
#endif
        _offset = Offset;
        _base = Base;
        _cachedBase = Base();
        _cache = (byte*)_cachedBase + Offset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void* Get()
    {
#if DEBUG
        if (!_initialized)
            throw new ArgumentOutOfRangeException();
#endif
        var currBase = _base();
        if (currBase != _cachedBase)
        {
            _cache = (void*) ((byte*)currBase + _offset);
            _cachedBase = currBase;
        }

        return _cache;
    }

    public bool Same(Pointer Ptr)
    {
        return Ptr._offset == _offset;
    }
}