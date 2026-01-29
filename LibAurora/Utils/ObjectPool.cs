using System;
using System.Collections.Concurrent;
namespace LibAurora.Utils;

public class ObjectPool<T> where T : class, new()
{
	private readonly ConcurrentBag<T> _objs;
	public ObjectPool(int initialSize)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(initialSize);
		_objs = [];
		for (var i = 0; i < initialSize; i++)
		{
			_objs.Add(new T());
		}
	}

	public T Get()
	{
		if(!_objs.TryTake(out var result))result=new T();
		return result;
	}
	
	public void Return(T obj)
	{
		_objs.Add(obj);
	}
}