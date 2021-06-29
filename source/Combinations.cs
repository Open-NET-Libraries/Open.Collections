using Open.Collections;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Open.Collections
{
	public class Combinations
	{

		/// <summary>
		/// Enumerate all possible m-size combinations of [0, 1, ..., bounds-1] array in lexicographic order (first [0, 1, 2, ..., length-1]).
		/// </summary>
		/// <param name="target"></param>
		/// <param name="length">The length of each result.</param>
		/// <param name="bounds">The maximum (excluded) value of the concecutive set of integers.</param>
		/// <returns>An enumerable that yields the target containing the next combination.</returns>
		public static IEnumerable<int[]> CopyTo(int[] target, int? length = null, int? bounds = null)
		{
			var len = length ?? target.Length;
			if (len > target.Length)
				throw new ArgumentOutOfRangeException(nameof(length), length, "Must be no more than the length of the buffer. ");
			var n = bounds ?? length;

			var stack = new Stack<int>(len);
			stack.Push(0);

			while (stack.Count > 0)
			{
				int index = stack.Count - 1;
				int value = stack.Pop();
				while (value < n)
				{
					target[index++] = value++;
					stack.Push(value);
					if (index != length) continue;
					yield return target;
					break;
				}
			}

		}


		/// <summary>
		/// Enumerate all possible m-size combinations of [0, 1, ..., bounds-1] array in lexicographic order (first [0, 1, 2, ..., length-1]).
		/// </summary>
		/// <param name="length">The length of each result.</param>
		/// <param name="bounds">The maximum (excluded) value of the concecutive set of integers.</param>
		/// <returns>An enumerable that yields the buffer (could be any length equal to or greater than the length) containing the next combination.</returns>
		public static IEnumerable<int[]> GetPossibleIndexesBuffer(int length, int? bounds = null)
		{

			var pool = ArrayPool<int>.Shared;
			var result = pool.Rent(length);
			try
			{
				return CopyTo(result, length, bounds);
			}
			finally
			{
				pool.Return(result);
			}
		}

		/// <summary>
		/// Enumerate all possible m-size combinations of [0, 1, ..., bounds-1] array in lexicographic order (first [0, 1, 2, ..., length-1]).
		/// </summary>
		/// <param name="length">The length of each result.</param>
		/// <param name="bounds">The maximum (excluded) value of the concecutive set of integers.</param>
		/// <returns>An enumerable that yields the buffer containing the next combination.</returns>
		public static IEnumerable<int[]> GetPossibleIndexes(int length, int? bounds = null)
		{
			foreach (var s in GetPossibleIndexesBuffer(length, bounds))
			{
				var a = new int[length];
				for (var i = 0; i < length; i++) a[i] = s[i];
				yield return a;
			}
		}

		public Combinations()
		{
			Indexes = GetIndexes().Memoize();
		}

		public IEnumerable<IEnumerable<T>> GetCombinations<T>(IEnumerable<T> values)
		{
			var source = values is IReadOnlyList<T> v ? v : values.ToImmutableArray();
			return Indexes[source.Count].Select(c => source.Arrange(c));
		}

		public IReadOnlyList<IReadOnlyList<T>> GetMemoizedCombinations<T>(IEnumerable<T> values)
		{
			var source = values is IReadOnlyList<T> v ? v : values.ToImmutableArray();
			return Indexes[source.Count].Select(c => source.Arrange(c).Memoize()).Memoize();
		}

		IEnumerable<IReadOnlyList<ImmutableArray<int>>> GetIndexes()
		{
			yield return ImmutableArray<ImmutableArray<int>>.Empty;
			yield return ImmutableArray.Create(ImmutableArray.Create(0));
			yield return ImmutableArray.Create(ImmutableArray.Create(0, 1), ImmutableArray.Create(1, 0));

			var i = 2;
			var indexes = new List<int> { 0, 1 };

		loop:
			indexes.Add(i);
			++i;
			yield return GetIndexesCore(indexes).Memoize();
			goto loop;
		}

		IEnumerable<ImmutableArray<int>> GetIndexesCore(IReadOnlyList<int> first)
		{
			var len = first.Count;
			var combinations = Indexes[len - 1];
			for (var i = 0; i < len; i++)
			{
				var builder = ImmutableArray.CreateBuilder<int>(len);
				foreach (var comb in combinations)
				{
					builder.Capacity = len;
					builder.Add(i);
					foreach (var c in comb)
					{
						var v = first[c < i ? c : (c + 1)];
						builder.Add(v);
					}
					yield return builder.MoveToImmutable();
				}
			}
		}

		readonly LazyList<IReadOnlyList<ImmutableArray<int>>> Indexes;

		public IReadOnlyList<ImmutableArray<int>> GetIndexes(int length) => Indexes[length];
	}
}
