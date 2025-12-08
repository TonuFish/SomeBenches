namespace SomeBenches.TupleSwapBench;

using System;

internal static class Swaps
{
	internal static int LocalTempSwap()
	{
		Span<int> numbers = [7, 6, 5];
		var temp = numbers[0];
		numbers[0] = numbers[1];
		numbers[1] = temp;
		return numbers[2];
	}

	internal static int LocalTupleSwap()
	{
		Span<int> numbers = [7, 6, 5];
		// Enregisters &numbers[1]
		(numbers[1], numbers[0]) = (numbers[0], numbers[1]);
		return numbers[2];
	}

	internal static int LocalTupleSwapMany()
	{
		Span<int> a = [7, 6, 5, 4, 3, 2];
		// Enregisters &a[3, 2, 1]
		(a[5], a[4], a[3], a[2], a[1], a[0]) = (a[0], a[1], a[2], a[3], a[4], a[5]);
		return a[2];
	}

	internal static int ParamTempSwap(Span<int> numbers)
	{
		var temp = numbers[0];
		numbers[0] = numbers[1];
		numbers[1] = temp;
		return numbers[2];
	}

	internal static int ParamTupleSwap_1_0(Span<int> numbers)
	{
		(numbers[1], numbers[0]) = (numbers[0], numbers[1]);
		return numbers[2];
	}

	internal static int ParamTupleSwap_0_1(Span<int> numbers)
	{
		// Bounds checks 0, then 1
		(numbers[0], numbers[1]) = (numbers[1], numbers[0]);
		return numbers[2];
	}

	internal static void ParamTupleSwapMany(Span<int> a)
	{
		(a[1], a[0], a[4], a[3], a[5], a[6]) = (a[7], a[5], a[255], a[4], a[10], a[1]);
	}
}
