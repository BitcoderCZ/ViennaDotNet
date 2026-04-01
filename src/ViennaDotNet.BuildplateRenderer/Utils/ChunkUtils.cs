// <copyright file="ChunkUtils.cs" company="BitcoderCZ">
// Copyright (c) BitcoderCZ. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;
using BitcoderCZ.Maths.Vectors;
using SharpNBT;

namespace ViennaDotNet.BuildplateRenderer.Utils;

internal static class ChunkUtils
{
	public const int Width = 16;
	public const int Height = 256;
	public const int SubChunkHeight = 16;

	public static readonly int[] EmptySubChunk = new int[Width * SubChunkHeight * Width];

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int2 BlockToChunk(int2 blockPosition)
		=> new int2(blockPosition.X >> 4, blockPosition.Y >> 4);

	public static int[] ReadBlockData(LongArrayTag nbt)
	{
		if (nbt.Count is 0)
		{
			return EmptySubChunk;
		}

		var resultData = GC.AllocateUninitializedArray<int>(Width * SubChunkHeight * Width);

		var longArray = nbt.Span;

		int bits = 4;

		for (int b = 4; b <= 64; b++)
		{
			int vpl = 64 / b;
			int expectedLength = (4096 + vpl - 1) / vpl;

			if (expectedLength == longArray.Length)
			{
				bits = b;
				break;
			}
		}

		int valuesPerLong = 64 / bits;
		long mask = (1L << bits) - 1;

		int dataIndex = 0;

		for (int i = 0; i < longArray.Length; i++)
		{
			long value = longArray[i];

			for (int j = 0; j < valuesPerLong; j++)
			{
				if (dataIndex >= 4096)
				{
					break;
				}

				resultData[dataIndex++] = (int)((value >> (j * bits)) & mask);
			}
		}

		return resultData;
	}
}
