#define DEBUGGING

using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SomeBenches.StackAllocOrRentBench;

// TODO: Move back to generic once it works.

[SkipLocalsInit]
public static class Program
{
	public static void Main()
	{
		const int length = 100; // <=128 for inline path
		/* using */ var buffer = Buffers.CreateCharBuffer(length, out var span);

#if DEBUGGING
        var spanPointer = Debugging.GetInternalPointer(ref span);
        var bufferPointer = Debugging.GetInternalPointer(in buffer);
        System.Diagnostics.Debug.Assert(spanPointer == bufferPointer); //! RIP
#endif // DEBUGGING

		UseSpan(span);
		buffer.Dispose();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void UseSpan(Span<char> span)
	{
	}
}

[SkipLocalsInit]
public static class Buffers
{
	public static CharBuffer CreateCharBuffer(int length, [UnscopedRef] out Span<char> span)
	{
		if (length > 128)
		{
			var rentedArray = ArrayPool<char>.Shared.Rent(length);
			span = rentedArray.AsSpan(0, length);
			span.Clear();
			return new(rentedArray);
		}

		CharBuffer buffer = new();
		span = buffer.GetSpan(length);

#if DEBUGGING
        var spanPointer = Debugging.GetInternalPointer(ref span);
        var bufferPointer = Debugging.GetInternalPointer(in buffer);
        System.Diagnostics.Debug.Assert(spanPointer == bufferPointer);
#endif // DEBUGGING

		return buffer;
	}
}

[SkipLocalsInit]
public readonly ref struct CharBuffer
{
	//? Probably shouldn't be readonly...
	internal readonly Buffer128<char> _buffer;
	internal readonly char[]? _rentedBuffer;

	internal CharBuffer(char[] rentedBuffer)
	{
		Unsafe.SkipInit(out _buffer);
		_rentedBuffer = rentedBuffer;
	}

	internal readonly Span<char> GetSpan(int length)
	{
		return MemoryMarshal.CreateSpan(ref Unsafe.AsRef(in _buffer._element0), length);
	}

	public readonly void Dispose()
	{
		if (_rentedBuffer is not null)
		{
			ArrayPool<char>.Shared.Return(_rentedBuffer);
		}
	}
}

[InlineArray(128)]
#pragma warning disable CA1815 // Override equals and operator equals on value types
public struct Buffer128<T> where T : struct
#pragma warning restore CA1815 // Override equals and operator equals on value types
{
	internal T _element0;
}

#if DEBUGGING
public static class Debugging
{
    public ref struct MySpanNow<T>
    {
        internal readonly ref T _reference;
        internal readonly int _length;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static unsafe nint GetInternalPointer(
        in CharBuffer buffer,
        [CallerMemberName] string callerMemberName = "")
    {
        fixed (char* pointer = &buffer._buffer[0])
        {
            Console.WriteLine($"{callerMemberName} method `buffer` internal pointer:\t{(nint)pointer:X}");
            return (nint)pointer;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static unsafe nint GetInternalPointer<T>(
        ref Span<T> span,
        [CallerMemberName] string callerMemberName = "")
        where T : struct
    {
        var disgusting = Unsafe.As<Span<T>, MySpanNow<T>>(ref span);
        var pointer = (nint)Unsafe.AsPointer(ref disgusting._reference);
        Console.WriteLine($"{callerMemberName} method `span` internal pointer:\t{pointer:X}");
        return pointer;
    }
}
#endif // DEBUGGING
