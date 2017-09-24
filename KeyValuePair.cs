namespace System.Collections.Generic
{
	public static class KeyValuePair
	{
		public static KeyValuePair<TKey,TValue> Create<TKey,TValue>(TKey key, TValue value)
		{
			return new KeyValuePair<TKey,TValue>(key,value);
		}
	}	
}