using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

namespace SomeBenches.ShiftMaskBench;

[SimpleJob(warmupCount: 20)]
[DisassemblyDiagnoser(maxDepth: 10, syntax: DisassemblySyntax.Intel)]
public class MicroBench
{
	private const uint Mask = 0xFEF;
	private const int Shift = 20;

	[Params(987_654_321UL)]
	public ulong Value { get; set; }

	[Benchmark(Baseline = true)]
	public bool CurrentMask()
	{
		return ((Value >> Shift) & Mask) != 0;
	}

	[Benchmark]
	public bool FoldedMask()
	{
		return (Value & (Mask << Shift)) != 0;
	}
}
