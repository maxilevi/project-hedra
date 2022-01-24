using System;
using System.Runtime.InteropServices;

namespace Hedra.Framework;

public unsafe class ResizableHeapAllocator : Allocator
{
    private void* _buffer;
    private int _maxSize;
    private int _currentSize;
    private int _step;
    
    public ResizableHeapAllocator(int MaxBufferSize, int Step) : base(MaxBufferSize)
    {
        _buffer = (void*) Marshal.AllocHGlobal(Step);
        _currentSize = Step;
        _maxSize = MaxBufferSize;
        _step = Step;
    }

    protected override void* CreateBuffer() => null;

    protected override void FreeBuffer()
    {
        Marshal.FreeHGlobal((IntPtr) _buffer);
        _buffer = null;
    }

    protected override Pointer GetPointer(int Offset, int Size)
    {
        var end = Offset + Size;
        if (end > _maxSize)
            throw new OutOfMemoryException("Tried to get pointer outside of buffer");

        while (end > _currentSize) 
            ResizeBuffer(_currentSize + _step);
        

        return new Pointer(() => (IntPtr)_buffer, Offset);
    }

    private void ResizeBuffer(int NewSize)
    {
        if (NewSize > _maxSize)
            throw new OutOfMemoryException("Tried to resize the buffer out of limits");

        _buffer =(void*) Marshal.ReAllocHGlobal((IntPtr)_buffer, (IntPtr)NewSize);
        _currentSize = NewSize;
    }
}