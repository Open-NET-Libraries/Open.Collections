using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Open.Collections.Tests;
public abstract class ParallelListTests<TList>
    : BasicListTests<TList>
    where TList : IList<int>
{
    protected ParallelListTests(TList list) : base(list)
    {
    }

    [Fact]
    public void ParallelAddThenRemove()
    {
        const int countAdd = 10000000;
        Collection.Clear();
        Parallel.For(0, countAdd, i => Collection.Add(i));
        Collection.Count.Should().Be(countAdd);

        const int countRemove = 900;
        Parallel.For(0, countRemove, i => Collection.Remove(i * 3));
        Collection.Count.Should().Be(countAdd - countRemove);
    }
}
