using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SomeBenches.SpanSliceBoundsBench;

[SimpleJob]
[DisassemblyDiagnoser(maxDepth: 10, syntax: DisassemblySyntax.Intel)]
public class BoundBench
{
	private const string Str = "Some Arbitrary String";

	[MethodImpl(MethodImplOptions.NoInlining)]
	public ReadOnlySpan<char> TestPositiveThenMinI32(int num, ReadOnlySpan<char> chars)
	{
		return (num >= 0) ? chars[..int.Min(chars.Length, num)] : chars;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public ReadOnlySpan<char> TestPredicateI32(int num, ReadOnlySpan<char> chars)
	{
		return (num >= 0 && num < chars.Length) ? chars[..num] : chars;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public ReadOnlySpan<char> TestPredicateU32(int num, ReadOnlySpan<char> chars)
	{
		return ((uint)num < chars.Length) ? chars[..num] : chars;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public ReadOnlySpan<char> TestMinU32(int num, ReadOnlySpan<char> chars)
	{
		return chars[..(int)uint.Min((uint)chars.Length, (uint)num)];
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public ReadOnlySpan<char> TestCreateRoS(int num, ReadOnlySpan<char> chars)
	{
		return MemoryMarshal.CreateReadOnlySpan(
			ref MemoryMarshal.GetReference(chars),
			(int)uint.Min((uint)chars.Length, (uint)num));
	}

	[Benchmark(Baseline = true)]
	public int PositiveThenMinI32()
	{
		var ret = 0;
		for (var ii = 0 ; ii < 4_000_000 ; ++ii)
		{
			var rv = TestPositiveThenMinI32(ii % 40, Str);
			ret += rv.Length;
		}
		return ret;
	}

	[Benchmark]
	public int PredicateI32()
	{
		var ret = 0;
		for (var ii = 0 ; ii < 4_000_000 ; ++ii)
		{
			var rv = TestPredicateI32(ii % 40, Str);
			ret += rv.Length;
		}
		return ret;
	}

	[Benchmark]
	public int PredicateU32()
	{
		var ret = 0;
		for (var ii = 0 ; ii < 4_000_000 ; ++ii)
		{
			var rv = TestPredicateU32(ii % 40, Str);
			ret += rv.Length;
		}
		return ret;
	}

	[Benchmark]
	public int MinU32()
	{
		var retVal = 0;
		for (var ii = 0 ; ii < 4_000_000 ; ++ii)
		{
			var rv = TestMinU32(ii % 40, Str);
			retVal += rv.Length;
		}
		return retVal;
	}

	[Benchmark]
	public int CreateRoS()
	{
		var retVal = 0;
		for (var ii = 0 ; ii < 4_000_000 ; ++ii)
		{
			var rv = TestCreateRoS(ii % 40, Str);
			retVal += rv.Length;
		}
		return retVal;
	}
}
