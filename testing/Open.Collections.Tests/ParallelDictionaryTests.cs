﻿using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Open.Collections.Tests;
public abstract class ParallelDictionaryTests<TDictionary>
    : BasicDictionaryTests<TDictionary>
    where TDictionary : IDictionary<int, int>
{
    protected ParallelDictionaryTests(TDictionary dictionary) : base(dictionary)
    {
    }

    [Fact]
    public void ParallelAddThenRemove()
    {
        const int countAdd = 10000000;
        Dictionary.Clear();
        Parallel.For(0, countAdd, i => Dictionary.Add(i, i));
        Dictionary.Count.Should().Be(countAdd);

        const int countRemove = 900;
        Parallel.For(0, countRemove, i => Dictionary.Remove(i * 3));
        Dictionary.Count.Should().Be(countAdd - countRemove);
    }
}
