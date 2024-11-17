using System.Collections;
using System.Collections.Concurrent;

namespace NeoCaptcha.AspnetCore.Entities
{

	public class FastCache<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IDisposable
	{
		private readonly ConcurrentDictionary<TKey, TtlValue> _dict = new ConcurrentDictionary<TKey, TtlValue>();

		private readonly Timer _cleanUpTimer;
		private readonly EvictionCallback _itemEvicted;


		public delegate void EvictionCallback(TKey key);

	
		public FastCache(int cleanupJobInterval = 10000, EvictionCallback itemEvicted = null)
		{
			_itemEvicted = itemEvicted;
			_cleanUpTimer = new Timer(s => { _ = EvictExpiredJob(); }, null, cleanupJobInterval, cleanupJobInterval);
		}

		private static SemaphoreSlim _globalStaticLock = new(1);
		private async Task EvictExpiredJob()
		{

			await _globalStaticLock.WaitAsync()
				.ConfigureAwait(false);
			try
			{
				EvictExpired();
			}
			finally { _globalStaticLock.Release(); }
		}


		public void EvictExpired()
		{
			if (!Monitor.TryEnter(_cleanUpTimer)) return;
			try
			{

				var currTime = Environment.TickCount64;

				foreach (var p in _dict)
				{
					if (!p.Value.IsExpired(currTime)) continue;
					_dict.TryRemove(p);
					OnEviction(p.Key);
				}
			}
			finally
			{
				Monitor.Exit(_cleanUpTimer);
			}
		}


		public int Count => _dict.Count;

		public void Clear() => _dict.Clear();

		public void AddOrUpdate(TKey key, TValue value, TimeSpan ttl)
		{
			var ttlValue = new TtlValue(value, ttl);

			_dict.AddOrUpdate(key, (k, c) => c, (k, v, c) => c, ttlValue);
		}


		public bool TryGet(TKey key, out TValue value)
		{
			value = default(TValue);

			if (!_dict.TryGetValue(key, out TtlValue ttlValue))
				return false; 

			if (ttlValue.IsExpired()) 
			{
				var kv = new KeyValuePair<TKey, TtlValue>(key, ttlValue);

				_dict.TryRemove(kv);

				OnEviction(key);

				return false;
			}

			value = ttlValue.Value;
			return true;
		}

		public bool TryAdd(TKey key, TValue value, TimeSpan ttl)
		{
			return !TryGet(key, out _) && _dict.TryAdd(key, new TtlValue(value, ttl));
		}

		private TValue GetOrAddCore(TKey key, Func<TValue> valueFactory, TimeSpan ttl)
		{
			var wasAdded = false; 
			var ttlValue = _dict.GetOrAdd(
				key,
				(k) =>
				{
					wasAdded = true;
					return new TtlValue(valueFactory(), ttl);
				});

			if (wasAdded) return ttlValue.Value; 
			if (ttlValue.ModifyIfExpired(valueFactory, ttl))
				OnEviction(key);

			return ttlValue.Value;
		}

		public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory, TimeSpan ttl)
			=> GetOrAddCore(key, () => valueFactory(key), ttl);

		public TValue GetOrAdd<TArg>(TKey key, Func<TKey, TArg, TValue> valueFactory, TimeSpan ttl, TArg factoryArgument)
			=> GetOrAddCore(key, () => valueFactory(key, factoryArgument), ttl);

		public TValue GetOrAdd(TKey key, TValue value, TimeSpan ttl)
			=> GetOrAddCore(key, () => value, ttl);

		public void Remove(TKey key)
		{
			_dict.TryRemove(key, out _);
		}

		public bool TryRemove(TKey key, out TValue value)
		{
			var res = _dict.TryRemove(key, out var ttlValue) && !ttlValue.IsExpired();
			value = res ? ttlValue.Value : default(TValue);
			return res;
		}

		/// <inheritdoc/>
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			var currTime = Environment.TickCount64; 
			foreach (var kvp in _dict)
			{
				if (!kvp.Value.IsExpired(currTime))
					yield return new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value.Value);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		private void OnEviction(TKey key)
		{
			if (_itemEvicted == null) return;

			Task.Run(() => 
			{
				try
				{
					_itemEvicted(key);
				}
				catch { } 
			});
		}

		private class TtlValue
		{
			public TValue Value { get; private set; }
			private long _tickCountWhenToKill;

			public TtlValue(TValue value, TimeSpan ttl)
			{
				Value = value;
				_tickCountWhenToKill = Environment.TickCount64 + (long)ttl.TotalMilliseconds;
			}

			public bool IsExpired() => IsExpired(Environment.TickCount64);

			public bool IsExpired(long currTime) => currTime > _tickCountWhenToKill;


			public bool ModifyIfExpired(Func<TValue> newValueFactory, TimeSpan newTtl)
			{
				var ticks = Environment.TickCount64;
				if (!IsExpired(ticks)) return false;
				_tickCountWhenToKill = ticks + (long)newTtl.TotalMilliseconds; 
				Value = newValueFactory();
				return true;
			}
		}


		private bool _disposedValue;
		void IDisposable.Dispose() => Dispose(true);

		protected virtual void Dispose(bool disposing)
		{
			if (_disposedValue) return;
			if (disposing)
			{
				_cleanUpTimer.Dispose();
			}

			_disposedValue = true;
		}
	}
}