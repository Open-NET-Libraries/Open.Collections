using System;
using System.Linq;
using Xunit;

namespace Open.Collections.Tests;

public class LazyListTests
{
	[Fact]
	public void LazyListSmokeTest()
	{
		{
            System.Collections.Generic.IEnumerable<int> e = Enumerable.Range(0, 5);
			Assert.Equal(5, e.Memoize().Count);
			Assert.Equal(5, e.MemoizeUnsafe().Count);
            Assert.True(e.Memoize().TryGetValueAt(1, out _));
            Assert.False(e.Memoize().TryGetValueAt(8, out _));
            Assert.False(e.MemoizeUnsafe().Contains(9));

            Assert.Throws<InvalidOperationException>(() => e.Memoize(true).IndexOf(9));
            Assert.Throws<InvalidOperationException>(() => e.Memoize(true).Count);
            Assert.Throws<ArgumentOutOfRangeException>(() => e.MemoizeUnsafe()[-1]);
            Assert.Throws<ArgumentOutOfRangeException>(() => e.MemoizeUnsafe().TryGetValueAt(-1, out _));
            Assert.Throws<ArgumentOutOfRangeException>(() => e.MemoizeUnsafe()[6]);

            int i = 0;
            foreach(int x in e.Memoize())
            {
                Assert.Equal(i++, x);
            }
        }

        {
            System.Collections.Generic.IEnumerable<int> e = Enumerable.Range(0, 30);
            LazyList<int> a = e.Memoize();
            LazyListUnsafe<int> b = e.MemoizeUnsafe();
			Assert.Equal(7, a[7]);
			Assert.Equal(7, b[7]);

			Assert.Equal(6, a.IndexOf(6));
			Assert.Equal(6, b.IndexOf(6));
			Assert.Equal(9, a.IndexOf(9));
			Assert.Equal(9, b.IndexOf(9));
		}
	}

    [Fact]
    public void DisposedTest()
    {
        var list = Enumerable.Range(0, 30).Memoize();
        list.Dispose();
        Assert.Throws<ObjectDisposedException>(() =>
        {
            foreach (int e in list) { }
        });
    }
}
