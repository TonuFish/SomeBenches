using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SomeBenches.ShiftMaskBench;

internal static class Proof
{
	/* THEORY:
	 * ((value >> shift) & mask) != 0 == (value & (mask << shift)) != 0
	 *
	 * PROVE:
	 * A range of `shift` values [0,N] exists for any given `mask`.
	 *
	 * AXIOMS:
	 * LZCNT(mask) == SIZEOF(mask) evaluates to false, as no `mask` bits are set.
	 *
	 * RESTRICTIONS:
	 * Only constant variables may be used:
	 * - `mask`
	 * - `shift`
	 * - SIZEOF(mask); Which is equivalent to SIZEOF(value)
	 * - LZCNT(mask)
	 *
	 * ================================================================================================================
	 *
	 * The first set bit of the mask must not enter the 0 padded region of `value` created by right shifting.
	 *
	 * LET...
	 * A = SIZEOF(value) - shift		; The index of the first non-padding bit
	 * B = |LZCNT(mask) - SIZEOF(mask)|	; The index of the first set bit in mask
	 * 
	 * T.F...
	 * A >= B
	 */

	public static void Run()
	{
		RunSigned();
		RunUnsigned();
	}

	[DoesNotReturn]
	private static void ThrowException(string? message = null)
	{
		throw new InvalidOperationException(message);
	}

	[DoesNotReturn]
	private static T ThrowException<T>(string? message = null)
	{
		throw new InvalidOperationException(message);
	}

	#region Signed

	private static readonly UInt128 _u128Mask = UInt128.Parse(
		"7FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF",
		System.Globalization.NumberStyles.AllowHexSpecifier);

	public static void RunSigned()
	{
		Console.WriteLine("Starting sbyte...");
		RunForSignedType<sbyte>();
		Console.WriteLine("...Finished sbyte.");

		Console.WriteLine("Starting short...");
		RunForSignedType<short>();
		Console.WriteLine("...Finished short.");
	}

	private static unsafe void RunForSignedType<T>()
		where T : unmanaged, IBinaryInteger<T>, ISignedNumber<T>, IMinMaxValue<T>, IComparisonOperators<T, T, bool>
	{
		for (var loopMask = T.MinValue ; loopMask <= T.MaxValue ; ++loopMask)
		{
			_ = Parallel.For(
				fromInclusive: 0,
				toExclusive: sizeof(T) * 8,
				body: shift =>
				{
					var mask = loopMask;
					if (mask.ShouldFoldMaskSigned(shift))
					{
						for (var value = T.Zero ; value <= T.MaxValue ; ++value)
						{
							var baseCase = ((value >> shift) & mask) != T.Zero;
							var foldedCase = (value & (mask << shift)) != T.Zero;

							if (baseCase != foldedCase)
							{
								ThrowException($"value: {value}, shift: {shift}, mask: {mask}");
								return;
							}

							if (value == T.MaxValue)
							{
								break;
							}
						}
					}
				});

			if (loopMask == T.MaxValue)
			{
				break;
			}
		}
	}

	private unsafe static bool ShouldFoldMaskSigned<T>(this T mask, int shift)
		where T : unmanaged, IBinaryInteger<T>, ISignedNumber<T>
	{
		if (sizeof(T) == 1)
		{
			const int sizeOfMask = 8 * 1;
			var a = sizeOfMask - shift;
			var b = Math.Abs(
				BitOperations.LeadingZeroCount((uint)(Unsafe.BitCast<T, byte>(mask) & (byte)0x7F))
				- (8 * 3)
				- sizeOfMask);
			return a >= b;
		}
		else if (sizeof(T) == 2)
		{
			const int sizeOfMask = 8 * 2;
			var a = sizeOfMask - shift;
			var b = Math.Abs(
				BitOperations.LeadingZeroCount((uint)(Unsafe.BitCast<T, ushort>(mask) & (ushort)0x7F_FF))
				- (8 * 2)
				- sizeOfMask);
			return a >= b;
		}
		else if (sizeof(T) == 4)
		{
			const int sizeOfMask = 8 * 4;
			var a = sizeOfMask - shift;
			var b = Math.Abs(
				BitOperations.LeadingZeroCount(Unsafe.BitCast<T, uint>(mask) & 0x7F_FF_FF_FFU)
				- sizeOfMask);
			return a >= b;
		}
		else if (sizeof(T) == 8)
		{
			const int sizeOfMask = 8 * 8;
			var a = sizeOfMask - shift;
			var b = Math.Abs(
				BitOperations.LeadingZeroCount(Unsafe.BitCast<T, ulong>(mask) & 0x7F_FF_FF_FF_FF_FF_FF_FFUL)
				- sizeOfMask);
			return a >= b;
		}
		// TODO: Int128
		//else if (sizeof(T) == 16)
		//{
			//const int sizeOfMask = 8 * 16;
			//var a = sizeOfMask - shift;
			//var b = ASDF;
			//return a >= b;
		//}

		return ThrowException<bool>();
	}

	#endregion Signed

	#region Unsigned

	public static void RunUnsigned()
	{
		Console.WriteLine("Starting byte...");
		RunForUnsignedType<byte>();
		Console.WriteLine("...Finished byte.");

		Console.WriteLine("Starting ushort...");
		RunForUnsignedType<ushort>();
		Console.WriteLine("...Finished ushort.");
	}

	private static unsafe void RunForUnsignedType<T>()
		where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>, IComparisonOperators<T, T, bool>
	{
		for (var loopMask = T.MinValue ; loopMask <= T.MaxValue ; ++loopMask)
		{
			_ = Parallel.For(
				fromInclusive: 0,
				toExclusive: sizeof(T) * 8,
				body: shift =>
				{
					var mask = loopMask;
					if (mask.ShouldFoldMaskUnsigned(shift))
					{
						for (var value = T.Zero ; value <= T.MaxValue ; ++value)
						{
							var baseCase = ((value >> shift) & mask) != T.Zero;
							var foldedCase = (value & (mask << shift)) != T.Zero;

							if (baseCase != foldedCase)
							{
								ThrowException($"value: {value}, shift: {shift}, mask: {mask}");
								return;
							}

							if (value == T.MaxValue)
							{
								break;
							}
						}
					}
				});

			if (loopMask == T.MaxValue)
			{
				break;
			}
		}
	}

	private unsafe static bool ShouldFoldMaskUnsigned<T>(this T mask, int shift)
		where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
	{
		if (sizeof(T) == 1)
		{
			const int sizeOfMask = 8 * 1;
			var a = sizeOfMask - shift;
			var b = Math.Abs(
				BitOperations.LeadingZeroCount((uint)Unsafe.BitCast<T, byte>(mask))
				- (8 * 3)
				- sizeOfMask);
			return a >= b;
		}
		else if (sizeof(T) == 2)
		{
			const int sizeOfMask = 8 * 2;
			var a = sizeOfMask - shift;
			var b = Math.Abs(
				BitOperations.LeadingZeroCount((uint)Unsafe.BitCast<T, ushort>(mask))
				- (8 * 2)
				- sizeOfMask);
			return a >= b;
		}
		else if (sizeof(T) == 4)
		{
			const int sizeOfMask = 8 * 4;
			var a = sizeOfMask - shift;
			var b = Math.Abs(BitOperations.LeadingZeroCount(Unsafe.BitCast<T, uint>(mask)) - sizeOfMask);
			return a >= b;
		}
		else if (sizeof(T) == 8)
		{
			const int sizeOfMask = 8 * 8;
			var a = sizeOfMask - shift;
			var b = Math.Abs(BitOperations.LeadingZeroCount(Unsafe.BitCast<T, ulong>(mask)) - sizeOfMask);
			return a >= b;
		}
		// TODO: UInt128
		//else if (sizeof(T) == 16)
		//{
			//const int sizeOfMask = 8 * 16;
			//var a = sizeOfMask - shift;
			//var b = ASDF;
			//return a >= b;
		//}

		return ThrowException<bool>();
	}

	#endregion Unsigned
}
