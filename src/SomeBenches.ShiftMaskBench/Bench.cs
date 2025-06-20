using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using System;
using System.Runtime.CompilerServices;
#if JETBRAINS
using BenchmarkDotNet.Diagnostics.dotTrace;
#endif

namespace SomeBenches.ShiftMaskBench;

[SimpleJob(warmupCount: 15)]
[DisassemblyDiagnoser(maxDepth: 10, syntax: DisassemblySyntax.Intel)]
#if JETBRAINS
[DotTraceDiagnoser]
#endif
public class Bench
{
	private const uint Mask = 0xFEF;
	private const int Shift = 20;

	private ulong[] Values { get; } = new ulong[32_768];

	[GlobalSetup]
	public void Setup()
	{
		Random rng = new(765);
		for (var ii = 0 ; ii < Values.Length ; ++ii)
		{
			Values[ii] = (ulong)rng.Next();
		}
	}

	[Benchmark(Baseline = true)]
	public int Current()
	{
		var count = 0;
		foreach (var value in Values)
		{
			if (CurrentMask(value))
			{
				++count;
			}
		}
		return count;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool CurrentMask(ulong value)
	{
		return ((value >> Shift) & Mask) != 0;
	}

	[Benchmark]
	public int Folded()
	{
		var count = 0;
		foreach (var value in Values)
		{
			if (FoldedMask(value))
			{
				++count;
			}
		}
		return count;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool FoldedMask(ulong value)
	{
		return (value & (Mask << Shift)) != 0;
	}
}
