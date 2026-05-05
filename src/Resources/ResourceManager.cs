using System;
using System.Collections.Generic;
using System.IO;
namespace LibAurora.Resources;

/// <summary>The low-level resource manager class. </summary>
public partial class ResourceManager(IResources services)
{
	private readonly Dictionary<Type, IResourceProcesser> _processers = new();
	/// <summary>The resource service. </summary>
	public IResources Services => services;

	/// <summary>Registers a resource processor for the specified type <typeparamref name="T"/>.</summary>
	public void RegisterProcesser<T>(ResourceProcesser<T> processer) => _processers[typeof(T)] = processer;
	/// <summary>Loads a resource of type <typeparamref name="T"/> from the specified path.</summary>
	public T LoadResource<T>(string resourcePath)
		=> _processers[typeof(T)] is not ResourceProcesser<T> processer
			? throw new ArgumentException($"Resource process type {typeof(T).FullName} not found")
			: processer.Load(resourcePath);
	/// <summary>Saves a resource of type <typeparamref name="T"/> to the specified path.</summary>
	public void SaveResource<T>(string resourcePath, T resource)
	{
		if (_processers[typeof(T)] is not ResourceProcesser<T> processer)
			throw new ArgumentException($"Resource process type {typeof(T).FullName} not found");
		processer.Save(resource, resourcePath);
	}
}
/// <summary>Internal interface for resource processor type-erasure.</summary>
internal interface IResourceProcesser;
/// <summary>
/// Typed resource processor that loads and saves resources of type <typeparamref name="T"/>
/// using the provided load and save delegates.
/// </summary>
public class ResourceProcesser<T>(ResourceManager manager, Func<Stream, T> load, Action<T, Stream> save) : IResourceProcesser
{
	/// <summary>Loads a resource from the specified path.</summary>
	public T Load(string path)
	{
		using var fs = manager.Services.GetResource(path);
		return load(fs);
	}

	/// <summary>Saves a resource to the specified path.</summary>
	public void Save(T resource, string path)
	{
		using var fs = manager.Services.GetResource(path);
		if (!fs.CanWrite) throw new IOException($"Resource {path} can't be written");
		save(resource, fs);
	}
}