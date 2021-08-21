using System;
using System.Linq;
using Xunit;

namespace Open.Collections.Tests
{
	public class LazyListTests
	{
		[Fact]
		public void LazyListSmokeTest()
		{
			{
				var e = Enumerable.Range(0, 5);
				Assert.Equal(5, e.Memoize().Count);
				Assert.Equal(5, e.MemoizeUnsafe().Count);

				Assert.Throws<InvalidOperationException>(() => e.Memoize(true).Count);
			}

			{
				var e = Enumerable.Range(0, 30);
				var a = e.Memoize();
				var b = e.MemoizeUnsafe();
				Assert.Equal(7, a[7]);
				Assert.Equal(7, b[7]);

				Assert.Equal(6, a.IndexOf(6));
				Assert.Equal(6, b.IndexOf(6));
				Assert.Equal(9, a.IndexOf(9));
				Assert.Equal(9, b.IndexOf(9));
			}
		}
	}
}
