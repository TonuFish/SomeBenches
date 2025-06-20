using BenchmarkDotNet.Running;

namespace SomeBenches.ShiftMaskBench;

internal static class Program
{
	private static void Main(string[] args)
	{
		if (args.Length != 0)
		{
			// dotnet run --project .\src\SomeBenches.ShiftMaskBench\ -c Release --filter '*Bench*' --affinity 1
			_ = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
			return;
		}

		Proof.Run();
	}
}
