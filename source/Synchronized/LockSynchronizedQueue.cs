namespace Open.Collections.Synchronized
{
	public class LockSynchronizedQueue<T> : Queue.Standard<T>, IQueue<T>
	{
		public LockSynchronizedQueue() : base()
		{

		}

		public new void Enqueue(T item)
		{
			lock (this) base.Enqueue(item);
		}

		public override bool TryDequeue(out T item)
		{
			if (Count != 0)
				lock (this)
					return base.TryDequeue(out item);

			item = default(T);
			return false;
		}

		public override bool TryPeek(out T item)
		{
			if (Count != 0)
				lock (this)
					return base.TryPeek(out item);

			item = default(T);
			return false;
		}
	}
}

