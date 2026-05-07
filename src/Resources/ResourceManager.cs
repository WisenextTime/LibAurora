using System;
using System.Collections.Generic;
using System.IO;
using LibAurora.Core;
namespace LibAurora.Resources;

/// <summary>The low-level resource manager class. </summary>
public static partial class ResourceManager
{
	private readonly static Dictionary<Type, IResourceProcesser> _processers = new();
	/// <summary>The resource service. </summary>
	private static IResources? _services;
	public static IResources Services => _services ?? throw new InvalidOperationException("ResourceManager is not initialized");

	/// <summary>Registers a resource processor for the specified type <typeparamref name="T"/>.</summary>
	public static void RegisterProcesser<T>(ResourceProcesser<T> processer) => _processers[typeof(T)] = processer;
	/// <summary>Loads a resource of type <typeparamref name="T"/> from the specified path.</summary>
	public static T LoadResource<T>(string resourcePath)
		=> _processers[typeof(T)] is not ResourceProcesser<T> processer
			? throw new ArgumentException($"Resource process type {typeof(T).FullName} not found")
			: processer.Load(resourcePath);
	/// <summary>Saves a resource of type <typeparamref name="T"/> to the specified path.</summary>
	public static void SaveResource<T>(string resourcePath, T resource)
	{
		if (_processers[typeof(T)] is not ResourceProcesser<T> processer)
			throw new ArgumentException($"Resource process type {typeof(T).FullName} not found");
		processer.Save(resource, resourcePath);
	}
	/// <summary> Initializes GPU resource processors. When <paramref name="graphics"/> is provided. </summary>
	public static void Init(ApplicationContext context)
	{
		_services = context.Resources;
		_services.RegisterAssembly(typeof(ResourceManager).Assembly);
		if (context.Graphics is { } graphics)
		{
			InitTextureProcessor(graphics);
			InitShaderProcessor(graphics);
		}
	}
}
/// <summary>Internal interface for resource processor type-erasure.</summary>
internal interface IResourceProcesser;
/// <summary>
/// Typed resource processor that loads and saves resources of type <typeparamref name="T"/>
/// using the provided load and save delegates.
/// </summary>
public class ResourceProcesser<T>(Func<Stream, T> load, Action<T, Stream> save) : IResourceProcesser
{
	/// <summary>Loads a resource from the specified path.</summary>
	public T Load(string path)
	{
		using var fs = ResourceManager.Services.GetResource(path);
		return load(fs);
	}

	/// <summary>Saves a resource to the specified path.</summary>
	public void Save(T resource, string path)
	{
		using var fs = ResourceManager.Services.GetResource(path);
		if (!fs.CanWrite) throw new IOException($"Resource {path} can't be written");
		save(resource, fs);
	}
}