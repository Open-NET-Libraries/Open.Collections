using FluentAssertions;
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
        const int count = 10000000;
        Dictionary.Clear();
        Parallel.For(0, count, i => Dictionary.Add(i, i));
        Dictionary.Count.Should().Be(count);

        Parallel.For(0, count, i => Dictionary.Remove(i));
        Dictionary.Count.Should().Be(0);
    }
}
