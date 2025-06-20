using BenchmarkDotNet.Running;

namespace SomeBenches.SpanSliceBoundsBench;

internal static class Program
{
	private static void Main(string[] args)
	{
		// dotnet run --project .\src\SomeBenches.SpanSliceBoundsBench\ -c Release --filter '*Bench*' --affinity 1
		_ = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
	}
}
